import { Injectable, EventEmitter, OnDestroy, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { Subject } from 'rxjs/Subject';
import { Observable } from 'rxjs/Observable';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';
import { Subscription } from 'rxjs/Subscription';

import { CookieService } from 'angular2-cookie/services/cookies.service';

import { OperationResult, LoginSession } from '../../shared/models';

import { InfoService } from './info.service';
import { EventManagerService } from './../../menu/services/event-manager.service';
import { HttpService } from './http.service';
import { WebSocketService } from './websocket.service';
import { Logger } from './logger.service';

@Injectable()
export class TaskbuilderService {
    defaultUrl = ['home'];
    errorMessages: string[] = [];
    redirectUrl = this.defaultUrl;

    tbConnection = new BehaviorSubject(false);
    connected: Subject<boolean> = new BehaviorSubject(false);

    subscriptions: Subscription[] = [];

    constructor(private httpService: HttpService,
        private socket: WebSocketService,
        private cookieService: CookieService,
        private logger: Logger,
        private router: Router,
        private infoService: InfoService,
        private eventManagerService: EventManagerService
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
        this.openTbConnection().delay(5000).repeat().takeUntil(this.tbConnection.filter(tbConnection => tbConnection === true)).subscribe();
    }

    openTbConnection(): Observable<boolean> {

        let authtoken = this.cookieService.get('authtoken');
        this.logger.debug("openTbConnection...", authtoken);

        return new Observable(observer => {
            this.httpService.openTBConnection({ authtoken: authtoken })
                .timeout(15000)
                .catch((error: any) => Observable.throw(error))
                .subscribe((tbRes: OperationResult) => {
                    this.logger.debug("openTBConnection result...", tbRes);

                    if (tbRes.error) {

                        this.logger.debug("error messages:", tbRes.messages);
                        // il TB c'è ma non riesce a collegare
                        this.logger.error("openTBConnection Connection Error - Reconnecting...");
                        this.tbConnection.next(false);
                        observer.next(false);
                        observer.complete();

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