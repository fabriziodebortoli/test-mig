import { Logger } from './logger.service';
import { EventManagerService } from './../../menu/services/event-manager.service';
import { LoginCompact } from './../../shared/models/login-compact.model';
import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { Observable } from 'rxjs/Observable';
import 'rxjs/add/observable/of';
import 'rxjs/add/operator/map';

import { CookieService } from 'angular2-cookie/services/cookies.service';

import { HttpService } from './http.service';

import { LoginSession } from './../../shared/models/login-session.model';
import { OperationResult } from './../../shared/models/operation-result.model';

@Injectable()
export class AuthService {
    private redirectUrl: string = '/';
    private loginUrl: string = '/login';
    private defaultUrl = '/home';
    private islogged: boolean = false;
    errorMessage: string = "";

    constructor(
        private httpService: HttpService,
        private logger: Logger,
        private router: Router,
        protected cookieService: CookieService,
        private eventManagerService: EventManagerService
    ) {

    }

    login(connectionData: LoginSession): Observable<boolean> {
        return this.httpService.login(connectionData).map((result: LoginCompact) => {
            this.islogged = result.success;

            if (!this.islogged) {
                this.errorMessage = result.message;
            }

            this.cookieService.put('authtoken', this.islogged ? result.authtoken : null);

            return this.islogged;
        });
    }

    isLogged(): Observable<boolean> {
        return this.httpService.isLogged({ authtoken: this.cookieService.get('authtoken') });
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
                    // this.httpService.closeTBConnection(); // TODO spostare in subscribe event logoff

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