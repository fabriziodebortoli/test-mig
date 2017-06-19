import { OnInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { LoginSession } from './../../../shared';
import { HttpService } from '../../../core/services/http.service';
import { LoginSessionService } from '../../../core/services/login-session.service';
import { CookieService } from 'angular2-cookie/services/cookies.service';
export declare class LoginComponent implements OnInit, OnDestroy {
    private loginSessionService;
    private cookieService;
    private router;
    private httpService;
    companies: any[];
    connectionData: LoginSession;
    working: boolean;
    constructor(loginSessionService: LoginSessionService, cookieService: CookieService, router: Router, httpService: HttpService);
    ngOnInit(): void;
    ngOnDestroy(): void;
    getCompaniesForUser(user: string): void;
    loadState(): void;
    saveState(): void;
    login(): void;
    keyDownFunction(event: any): void;
}
