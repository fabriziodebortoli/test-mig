import { Http, Response } from '@angular/http';
import { Observable } from 'rxjs/Rx';
import { CookieService } from 'angular2-cookie/services/cookies.service';
import { OperationResult } from '../../shared/models/operation-result.model';
import { LoginSession } from '../../shared/models/login-session';
import { UtilsService } from './utils.service';
import { UrlService } from './url.service';
import { Logger } from './logger.service';
import { ErrorObservable } from 'rxjs/observable/ErrorObservable';
export declare class HttpService {
    protected http: Http;
    protected utils: UtilsService;
    protected logger: Logger;
    protected urlService: UrlService;
    protected cookieService: CookieService;
    constructor(http: Http, utils: UtilsService, logger: Logger, urlService: UrlService, cookieService: CookieService);
    createOperationResult(res: Response): OperationResult;
    isLogged(): Observable<string>;
    getInstallationInfo(): Observable<any>;
    login(connectionData: LoginSession): Observable<OperationResult>;
    getCompaniesForUser(user: string): Observable<any>;
    isActivated(application: string, functionality: string): Observable<any>;
    loginCompact(connectionData: LoginSession): Observable<OperationResult>;
    logoff(): Observable<OperationResult>;
    logout(): Observable<OperationResult>;
    runDocument(ns: String, args?: string): void;
    runReport(ns: String): Observable<any>;
    postData(url: string, data: Object): Observable<Response>;
    getComponentUrl(url: string): string;
    getBaseUrl(): string;
    getDocumentBaseUrl(): string;
    getMenuBaseUrl(): string;
    getAccountManagerBaseUrl(): string;
    getMenuServiceUrl(): string;
    getEnumsServiceUrl(): string;
    protected handleError(error: any): ErrorObservable;
    getEnumsTable(): Observable<any>;
}
