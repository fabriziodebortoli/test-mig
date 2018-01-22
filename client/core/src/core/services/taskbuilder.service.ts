import { ThemeService } from './theme.service';
import { AuthService } from './auth.service';
import { EventManagerService } from './event-manager.service';
import { DialogService } from '@progress/kendo-angular-dialog';
import { Injectable, EventEmitter, OnDestroy, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, Subject, BehaviorSubject, Subscription } from '../../rxjs.imports';

import { OperationResult } from './../../shared/models/operation-result.model';

import { InfoService } from './info.service';
import { HttpService } from './http.service';
import { WebSocketService } from './websocket.service';
import { Logger } from './logger.service';
import { ConnectionStatus } from './../../shared/models/connection-status.enum';
import { DiagnosticService } from './../../core/services/diagnostic.service';

@Injectable()
export class TaskBuilderService {
    defaultUrl = ['home'];
    errorMessages: string[] = [];
    redirectUrl = this.defaultUrl;

    timeout = 120000;
    tbConnection = new BehaviorSubject(false);
    connected: Subject<boolean> = new BehaviorSubject(false);

    subscriptions: Subscription[] = [];
    public _connectionStatus = ConnectionStatus.None;
    public connectionStatus = new EventEmitter<ConnectionStatus>();
    constructor(
        public httpService: HttpService,
        public socket: WebSocketService,
        public logger: Logger,
        public router: Router,
        public infoService: InfoService,
        public eventManagerService: EventManagerService,
        public authService: AuthService,
        private themeService: ThemeService,
        private diagnosticService: DiagnosticService
    ) {

        // Connessione WS quando viene aperta connessione al tbLoader
        this.subscriptions.push(this.tbConnection.subscribe(tbConnection => {
            this.logger.debug("tbConnection subscription, se true devo collegarmi al WS", tbConnection)
            if (tbConnection) {
                if (!infoService.isDesktop) {
                    this.socket.wsConnect();
                }
                else {
                    this.connected.next(true);
                }
            }
        }));

        // Vera connessione quando anche il WS è connesso
        this.subscriptions.push(this.socket.open.subscribe(() => {
            this.connected.next(true);
            this.setConnectionStatus(ConnectionStatus.Connected);

        }));

        // riconnessione alla chiusura del WS
        this.subscriptions.push(this.socket.close.subscribe(() => {
            this.tbConnection.next(false);
            this.connected.next(false);

            this.setConnectionStatus(ConnectionStatus.Disconnected);
            this.logger.debug("Riconnessione in corso...")
            let sub = this.openTbConnectionAndShowDiagnostic().subscribe(res => { sub.unsubscribe(); });

        }));

        this.subscriptions.push(this.eventManagerService.loggingOff.subscribe(() => {
            this.logger.debug("LoggedOut")
            this.tbConnection.next(false);
            this.connected.next(false);
        }));

    }

    setConnectionStatus(status: ConnectionStatus) {
        this._connectionStatus = status;
        this.connectionStatus.emit(status);
    }
    dispose() {
        this.tbConnection.next(false);
        this.connected.next(false);
    }
    openTbConnectionAndShowDiagnostic(): Observable<boolean> {
        return new Observable(observer => {
            let sub = this.openTbConnection().subscribe(res => {
                sub.unsubscribe();
                if (res.error && res.messages) {
                    let sub = this.diagnosticService.showDiagnostic(res.messages).subscribe(obs => {
                        sub.unsubscribe();
                        observer.next(!res.error);
                        observer.complete();
                    });
                } else {
                    observer.next(!res.error);
                    observer.complete();
                }
            });
        });
    }
    openTbConnection(): Observable<OperationResult> {

        this.setConnectionStatus(ConnectionStatus.Connecting);
        const authtoken = sessionStorage.getItem('authtoken');
        const isDesktop = this.infoService.isDesktop;
        return new Observable(observer => {
            this.httpService.initTBLogin({ authtoken: authtoken, isDesktop: isDesktop })
                .timeout(this.timeout)
                .catch((error: any) => Observable.throw(error))
                .subscribe((tbRes: OperationResult) => {
                    this.logger.debug('initTBLogin result...', tbRes);

                    if (tbRes.error) {
                        this.logger.debug('error messages:', tbRes.messages);
                        this.tbConnection.next(false);
                        this.setConnectionStatus(ConnectionStatus.Unavailable);
                        this.socket.setWsConnectionStatus(ConnectionStatus.Unavailable);
                    } else {
                        this.themeService.loadThemes();
                        this.tbConnection.next(true);
                    }

                    observer.next(tbRes);
                    observer.complete();

                }, (error) => {
                    this.logger.error("initTBLogin Connection failed", error);
                    this.tbConnection.next(false);
                    let res = new OperationResult(true, [{ text: error }]);
                    observer.next(res);
                    observer.complete();
                });
        })
    }

    closeConnection() {
        let authtoken = sessionStorage.getItem('authtoken');
        this.logger.debug("closeTbConnection...", authtoken);

        this.httpService.closeTBConnection({ authtoken: authtoken }).subscribe();
    }

}