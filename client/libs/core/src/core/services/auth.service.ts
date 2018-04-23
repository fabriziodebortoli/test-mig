import { Injectable } from '@angular/core';
import { Router } from '@angular/router';

import { Observable } from '../../rxjs.imports';

import { LoginCompact } from './../../shared/models/login-compact.model';
import { LoginSession } from './../../shared/models/login-session.model';
import { OperationResult } from './../../shared/models/operation-result.model';

import { Logger } from './logger.service';
import { InfoService } from './info.service';
import { HttpService } from './http.service';
import { EventManagerService } from './event-manager.service';

@Injectable()
export class AuthService {
    public redirectUrl: string = '/';
    public loginUrl: string = '/login';
    public defaultUrl = '/';
    public islogged: boolean = false;
    errorMessage: string = "";

    constructor(
        public httpService: HttpService,
        public infoService: InfoService,
        public logger: Logger,
        public router: Router,
        public eventManagerService: EventManagerService
    ) { }

    login(connectionData: LoginSession): Observable<LoginCompact> {
        this.errorMessage = "";
        return this.httpService.login(connectionData).map((result: LoginCompact) => {
            this.islogged = result.success;

            if (!this.islogged) {
                this.errorMessage = result.message;
            }

            sessionStorage.setItem('authtoken', this.islogged ? result.authtoken : null);

            this.eventManagerService.emitLoggedIn();

            return result;
        });
    }

    isLogged(): Observable<boolean> {
        return this.httpService.isLogged({ authtoken: sessionStorage.getItem('authtoken') }).map(isLogged => {
            if (!isLogged) {
                localStorage.removeItem('authtoken');
            }
            return isLogged;
        });
    }

    changePassword(user: string, oldPassword:string, newPassword: string): Observable<LoginCompact> {
        return this.httpService.changePassword({ user: user, oldPassword: oldPassword, newPassword: newPassword }).map(result => {
            return result;
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
    getServerDownPageUrl(): string {
        return '/server-down';
    }

    logout(): void {
        let subs = this.httpService.logoff({ authtoken: sessionStorage.getItem('authtoken') }).subscribe(
            loggedOut => {
                if (loggedOut) {
                    this.eventManagerService.emitloggingOff();
                    this.islogged = !loggedOut;
                    sessionStorage.removeItem('authtoken');

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