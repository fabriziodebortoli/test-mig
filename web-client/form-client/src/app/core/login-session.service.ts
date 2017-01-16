import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { Observable } from 'rxjs';

import { CookieService } from 'angular2-cookie/services/cookies.service';

import { OperationResult } from './operation.result';
import { LoginSession } from './../shared/models/login-session';

import { HttpService } from './http.service';
import { WebSocketService } from './websocket.service';

import { Logger } from 'libclient';

@Injectable()
export class LoginSessionService {

    connected: boolean = false;
    errorMessages: string[] = [];

    constructor(private httpService: HttpService,
        private socket: WebSocketService,
        private cookieService: CookieService,
        private logger: Logger,
        private router: Router) {

        this.socket.close.subscribe(() => { this.setConnected(false); });
        this.isLogged();
    }

    isLogged(): void {
        let subs = this.httpService.isLogged().subscribe(
            isLogged => {
                this.logger.debug('isLogged returns: ' + isLogged);

                if (!isLogged) {
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
            }
        );
    }

    login(connectionData: LoginSession): Observable<OperationResult> {
        return Observable.create(observer => {
            this.httpService.login(connectionData).subscribe(
                result => {
                    this.setConnected(!result.error);
                    this.errorMessages = result.messages;
                    ;
                    if (this.isConnected) {
                        this.socket.wsConnect();
                    }
                    observer.next(result);
                    observer.complete();

                },
                error => {
                    this.logger.error('login HTTP error: ' + error);
                    this.errorMessages = [error];
                    observer.error(error);
                    observer.complete();
                }

            );

        });
    }

    logout(): void {
        let subscription = this.httpService.logout().subscribe(
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

    isConnected(): boolean {
        return this.connected;
    }
    setConnected(val: boolean) {
        this.connected = val;
        if (this.connected) {
            this.router.navigate(['home'], { skipLocationChange: false, replaceUrl: false });
        }
        else {
            this.router.navigate(['login'], { skipLocationChange: false, replaceUrl: false });
        }
    }
}