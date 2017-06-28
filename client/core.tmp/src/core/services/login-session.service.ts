﻿import { Injectable } from '@angular/core';
import { Router } from '@angular/router';

import { Observable } from 'rxjs';

import { CookieService } from 'angular2-cookie/services/cookies.service';

import { OperationResult } from '../../shared/models/operation-result.model';
import { LoginSession } from '../../shared/models/login-session';
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

        const subs = this.socket.close.subscribe(() => {
            subs.unsubscribe();
            this.setConnected(false);
        });

        this.checkIfLogged();
    }

    checkIfLogged() {
        const subs = this.httpService.isLogged().subscribe(
            ret => {
                if (!ret) {
                    this.setConnected(false);
                } else {
                    this.logger.debug('Just logged in');
                    this.setConnected(true);
                    this.socket.wsConnect();
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
                        this.socket.wsConnect();
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
        const subscription = this.httpService.logout().subscribe(
            loggedOut => {
                this.logger.debug('logout returns: ' + loggedOut);
                this.setConnected(!loggedOut);
                this.socket.wsClose();
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