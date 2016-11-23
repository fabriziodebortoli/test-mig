import { Observable } from 'rxjs';
import { HttpService } from './http.service';
import { LoginSession } from 'tb-shared';
import { Injectable } from '@angular/core';
import { WebSocketService } from './websocket.service';
import {Logger} from 'libclient';
import { CookieService } from 'angular2-cookie/services/cookies.service';

@Injectable()
export class LoginSessionService {
    connected: boolean = false;
    error: string;
    constructor(private httpService: HttpService,
        private socket: WebSocketService,
        private cookieService: CookieService,
        private logger: Logger) {

        this.isLogged();
    }

    isLogged(): void {
        let subs = this.httpService.isLogged().subscribe(
            isLogged => {
                this.logger.debug('isLogged returns: ' + isLogged);

                if (!isLogged) {
                    this.connected = false;
                } else {
                    this.logger.debug('Just logged in');
                    this.connected = true;
                    this.socket.wsConnect();
                }
                subs.unsubscribe();
            },
            error => {
                this.error = error;
                this.logger.error('isLogged HTTP error: ' + error);
                subs.unsubscribe();
            }
        );
    }

  login(connectionData: LoginSession): Observable<boolean> {
        return Observable.create(observer => {
            this.httpService.login(connectionData).subscribe(
                logged => {
                    this.logger.debug('login returns: ' + logged);
                    this.connected = logged;
                    if (logged) {
                        this.socket.wsConnect();
                    }
                    observer.next(logged);
                    observer.complete();

                },
                error => {
                    this.logger.error('login HTTP error: ' + error);
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
                this.connected = !loggedOut;
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
}
