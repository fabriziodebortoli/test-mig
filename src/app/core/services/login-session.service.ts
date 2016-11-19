import { LoginSession } from './../models/login-session';
import { Injectable } from '@angular/core';

import { HttpService } from '../services/http.service';
import { WebSocketService } from './websocket.service';
import { Logger } from './logger.service';
import { CookieService } from 'angular2-cookie/services/cookies.service';

@Injectable()
export class LoginSessionService {
    connected: boolean = false;

    constructor(private httpService: HttpService,
        private socket: WebSocketService,
        private cookieService: CookieService,
        private logger: Logger) {

        this.isLogged();
    }

    isLogged(): void {
        this.httpService.isLogged().subscribe(
            isLogged => {
                this.logger.debug('isLogged returns: ' + isLogged);

                if (!isLogged) {
                    // per effettuare la login automaticamente
                    // this.login();

                    this.connected = false;
                } else {
                    this.logger.debug('Just logged in');
                    this.connected = true;
                    this.socket.wsConnect();
                }
            },
            error => this.logger.error('isLogged HTTP error: ' + error)
        );
    }

    login(connectionData: LoginSession): void {
        this.httpService.login(connectionData).subscribe(
            logged => {
                this.logger.debug('login returns: ' + logged);
                this.connected = logged;
                if (logged) {
                    this.socket.wsConnect();
                }
            },
            error => this.logger.error('login HTTP error: ' + error)
        );
    }

    logout(): void {
        this.httpService.logout().subscribe(
            loggedOut => {
                this.logger.debug('logout returns: ' + loggedOut);
                this.connected = !loggedOut;
                this.socket.wsClose();
                this.cookieService.remove('authtoken');
            },
            error => this.logger.error('logout HTTP error: ' + error)
        );
    }

    isConnected(): boolean {
        return this.connected;
    }
}
