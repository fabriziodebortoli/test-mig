import { OperationResult } from './operation.result';
import { Observable } from 'rxjs';
import { HttpService } from './http.service';
import { LoginSession } from 'tb-shared';
import { Injectable } from '@angular/core';
import { WebSocketService } from './websocket.service';
import { Logger } from 'libclient';
import { CookieService } from 'angular2-cookie/services/cookies.service';

@Injectable()
export class LoginSessionService {
    connected: boolean = false;
    errorMessages: string[] = [];
    constructor(private httpService: HttpService,
        private socket: WebSocketService,
        private cookieService: CookieService,
        private logger: Logger) {

        this.isLogged();

        this.socket.on('close', () => { this.connected = false; });
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
                    this.connected = !result.error;
                    this.errorMessages = result.messages;
                    ;
                    if (this.connected) {
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
