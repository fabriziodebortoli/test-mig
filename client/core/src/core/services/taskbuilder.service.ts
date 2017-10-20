﻿import { AuthService } from './auth.service';
import { EventManagerService } from './event-manager.service';
import { DialogService } from '@progress/kendo-angular-dialog';
import { Injectable, EventEmitter, OnDestroy, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, Subject, BehaviorSubject, Subscription } from '../../rxjs.imports';
import { CookieService } from 'ngx-cookie';

import { OperationResult } from './../../shared/models/operation-result.model';

import { InfoService } from './info.service';
import { HttpService } from './http.service';
import { WebSocketService } from './websocket.service';
import { DiagnosticService } from './diagnostic.service';
import { Logger } from './logger.service';

@Injectable()
export class TaskbuilderService {
    defaultUrl = ['home'];
    errorMessages: string[] = [];
    redirectUrl = this.defaultUrl;

    timeout = 30000;
    tbConnection = new BehaviorSubject(false);
    connected: Subject<boolean> = new BehaviorSubject(false);

    subscriptions: Subscription[] = [];
    stopConnection: boolean = false;
    constructor(
        public httpService: HttpService,
        public socket: WebSocketService,
        public cookieService: CookieService,
        public logger: Logger,
        public router: Router,
        public infoService: InfoService,
        public eventManagerService: EventManagerService,
        public diagnosticService: DiagnosticService,
        public authService: AuthService
    ) {

        // Connessione WS quando viene aperta connessione al tbLoader
        this.subscriptions.push(this.tbConnection.subscribe(tbConnection => {
            this.logger.debug("tbConnection subscription, se true devo collegarmi al WS", tbConnection)
            if (tbConnection) {
                if (!infoService.isDesktop) {
                    this.socket.wsConnect();
                }
                else
                    this.connected.next(true);
            }
        }));

        // Vera connessione quando anche il WS è connesso
        this.subscriptions.push(this.socket.open.subscribe(() => {
            this.connected.next(true);
        }));

        // riconnessione alla chiusura del WS
        this.subscriptions.push(this.socket.close.subscribe(() => {
            this.tbConnection.next(false);
            this.connected.next(false);

            this.logger.debug("Riconnessione in corso...")
            this.openConnection();

        }));

        this.subscriptions.push(this.eventManagerService.loggingOff.subscribe(() => {
            this.logger.debug("LoggedOut")
            this.tbConnection.next(false);
            this.connected.next(false);
        }));

    }

    dispose() {
        this.tbConnection.next(false);
        this.connected.next(false);
    }

    // provo ad aprire connessione TB
    openConnection() {
        this.openTbConnection().delay(this.timeout).repeat().takeUntil(this.tbConnection.filter(tbConnection => tbConnection === true || this.stopConnection === true)).subscribe();
    }

    openTbConnection(): Observable<boolean> {

        let authtoken = this.cookieService.get('authtoken');
        this.logger.debug("openTbConnection...", authtoken);
        let isDesktop = this.infoService.isDesktop;
        return new Observable(observer => {
            this.httpService.openTBConnection({ authtoken: authtoken, isDesktop: isDesktop })
                .timeout(this.timeout)
                .catch((error: any) => Observable.throw(error))
                .subscribe((tbRes: OperationResult) => {
                    this.logger.debug("openTBConnection result...", tbRes);

                    if (tbRes.error) {

                       
                        this.stopConnection = true;

                        this.logger.debug("error messages:", tbRes.messages);
                        // il TB c'è ma non riesce a collegare
                        this.logger.error("openTBConnection Connection Error - Reconnecting...");
                        this.tbConnection.next(true); //passo true perchè la connessione è finita, anche se in maniera fallimentare, in questo modo stoppo il loading
                        observer.next(true);
                        observer.complete();

                        this.diagnosticService.showDiagnostic(tbRes.messages); //qui dovrei fare logout
                        this.authService.logout();

                    } else {
                        this.logger.debug("TbLoader Connected...")
                        this.tbConnection.next(true);

                        observer.next(true);
                        observer.complete();
                    }

                }, (error) => {
                    this.logger.error("openTBConnection Connection failed", error);
                    this.tbConnection.next(false);
                    observer.next(false);
                    observer.complete();
                });
        })
    }

    closeConnection() {
        let authtoken = this.cookieService.get('authtoken');
        this.logger.debug("closeTbConnection...", authtoken);

        this.httpService.closeTBConnection({ authtoken: authtoken }).subscribe();
    }

}