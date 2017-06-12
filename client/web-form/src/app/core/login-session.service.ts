import { Observable } from 'rxjs';
import { Injectable, forwardRef, Inject } from '@angular/core';
import { Router } from '@angular/router';

import { CookieService } from 'angular2-cookie/services/cookies.service';

import { OperationResult } from './operation.result';
import { LoginSession } from './../shared/models/login-session';
import { HttpService } from './http.service';
import { WebSocketService } from './websocket.service';

import { Logger } from './logger.service';

@Injectable()
export class LoginSessionService {
    defaultUrl = ['home'];
    connected = false;
    errorMessages: string[] = [];
    redirectUrl = this.defaultUrl;

    constructor(private httpService: HttpService,
        private socket: WebSocketService,
        private cookieService: CookieService,
        private logger: Logger,
        private router: Router) {

        this.checkIfLogged();

        const subs = this.socket.close.subscribe(() => {
             this.connected = false;
            this.openTbConnection(true);
        });
        socket.loginSessionService = this;
    }

    openTbConnection(retry: boolean = false): Observable<boolean> {
        return Observable.create(observer => {
            const tbSubs = this.httpService.openTBConnection().subscribe(tbRes => {
                if (tbRes.error) {
                    this.logger.debug(tbRes.messages);
                    if (retry) {
                        setTimeout(function () {
                            this.openTbConnection(true);
                        }, 5000);
                    }
                    observer.next(false);
                } else {
                    const wsSubs = this.socket.open.subscribe(() => {
                        wsSubs.unsubscribe();
                        observer.next(true);
                    });
                    this.socket.wsConnect();

                }
                observer.complete();
                tbSubs.unsubscribe();
            });
        });

    }

    checkIfLogged() {
        const subs = this.httpService.isLogged().subscribe(
            ret => {
                if (!ret) {
                    this.setConnected(false);
                } else {
                    this.logger.debug('Just logged in');
                    this.setConnected(true);
                    this.openTbConnection();

                }
                subs.unsubscribe();
            },
            error => {
                this.errorMessages = [error];
                this.logger.error('isLogged HTTP error: ' + error);
                subs.unsubscribe();
                this.setConnected(false);
            }
        );
    }
    login(connectionData: LoginSession): Observable<OperationResult> {
        return Observable.create(observer => {
            const subs = this.httpService.login(connectionData).subscribe(
                result => {
                    this.setConnected(!result.error);
                    this.errorMessages = result.messages;
                    if (this.connected) {
                        this.openTbConnection();
                    }
                    observer.next(result);
                    observer.complete();
                    subs.unsubscribe();

                },
                error => {
                    this.logger.error('login HTTP error: ' + error);
                    this.errorMessages = [error];
                    observer.error(error);
                    observer.complete();
                    subs.unsubscribe();
                }

            );

        });
    }

    logout(): void {
        const subscription = this.httpService.logoff().subscribe(
            loggedOut => {
                this.logger.debug('logout returns: ' + loggedOut);
                this.setConnected(!loggedOut);
                this.httpService.closeTBConnection();
                // this.socket.wsClose(); lo chiude il server facendo logoff
                this.cookieService.remove('authtoken');
                subscription.unsubscribe();
            },
            error => {
                this.logger.error('logout HTTP error: ' + error);
                subscription.unsubscribe();
            }
        );
    }
    isConnected() {
        return this.connected;
    }
    setConnected(val: boolean) {
        this.connected = val;
        let url = this.connected ? this.redirectUrl : ['login'];
        if (url.length === 0) {
            url = this.defaultUrl;
        }

        this.router.navigate(url, { skipLocationChange: false, replaceUrl: false });
    }
}