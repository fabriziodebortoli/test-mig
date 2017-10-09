import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { Observable } from 'rxjs/Observable';
import 'rxjs/add/observable/of';
import 'rxjs/add/operator/map';

import { CookieService } from 'angular2-cookie/services/cookies.service';

import { LoginCompact } from './../../shared/models/login-compact.model';
import { LoginSession } from './../../shared/models/login-session.model';
import { OperationResult } from './../../shared/models/operation-result.model';

import { Logger } from './logger.service';
import { EventManagerService } from './../../menu/services/event-manager.service';
import { HttpService } from './http.service';

@Injectable()
export class AuthService {
    public redirectUrl: string = '/';
    public loginUrl: string = '/login';
    public defaultUrl = '/home';
    public islogged: boolean = false;
    errorMessage: string = "";

    constructor(
        public httpService: HttpService,
        public logger: Logger,
        public router: Router,
        public cookieService: CookieService,
        public eventManagerService: EventManagerService
    ) {

    }

    login(connectionData: LoginSession): Observable<LoginCompact> {
        this.errorMessage = "";
        return this.httpService.login(connectionData).map((result: LoginCompact) => {
            this.islogged = result.success;

            if (!this.islogged) {
                this.errorMessage = result.message;
            }

            this.cookieService.put('authtoken', this.islogged ? result.authtoken : null);

            this.eventManagerService.emitLoggedIn();

            return result;
        });
    }

    isLogged(): Observable<boolean> {
        return this.httpService.isLogged({ authtoken: this.cookieService.get('authtoken') }).map(isLogged => {
            if (!isLogged) {
                this.cookieService.remove('authtoken');
            }
            return isLogged;
        });
    }
    getRedirectUrl(): string {
        return this.redirectUrl;
    }
    setRedirectUrl(url: string): void {
        this.redirectUrl = url;
    }
    getLoginUrl(): string {
        return this.loginUrl;
    }
    getDefaultUrl(): string {
        return this.defaultUrl;
    }

    logout(): void {
        let subs = this.httpService.logoff({ authtoken: this.cookieService.get('authtoken') }).subscribe(
            loggedOut => {
                if (loggedOut) {
                    this.eventManagerService.emitloggingOff();
                    this.islogged = !loggedOut;
                    this.cookieService.remove('authtoken');

                    this.router.navigate([this.getLoginUrl()]);
                }

                subs.unsubscribe();


            },
            error => {
                this.logger.error('logout HTTP error: ' + error);
                subs.unsubscribe();
            }
        );
    }
}