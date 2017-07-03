import { Injectable } from '@angular/core';
import { Http, Response, Headers } from '@angular/http';
import { Observable } from 'rxjs/Rx';

import { CookieService } from 'angular2-cookie/services/cookies.service';

import { OperationResult } from '../../shared/models/operation-result.model';
import { LoginSession } from '../../shared/models/login-session';
import { UtilsService } from './utils.service';
import { UrlService } from './url.service';

import { Logger } from './logger.service';
import { ErrorObservable } from 'rxjs/observable/ErrorObservable';

@Injectable()
export class HttpService {

    constructor(
        protected http: Http,
        protected utils: UtilsService,
        protected logger: Logger,
        protected urlService: UrlService,
        protected cookieService: CookieService) {
    }

    createOperationResult(res: Response): OperationResult {
        let jObject = res.ok ? res.json() : null;
        let ok = jObject && jObject.success === true;
        let messages = jObject ? jObject.messages : [];
        return new OperationResult(!ok, messages);
    }
    isLogged(): Observable<string> {
        return this.postData(this.getMenuBaseUrl() + 'isLogged/', {})
            .map((res: Response) => {
                return res.ok && res.json().success === true;
            })
            .catch(this.handleError);
    }
    getInstallationInfo(): Observable<any> {
        return this.postData(this.urlService.getBackendUrl() + 'tb/menu/getInstallationInfo/', {})
            .map((res: any) => {
                return res.json();
            })
            .catch(this.handleError);
    }
    login(connectionData: LoginSession): Observable<OperationResult> {
        return this.postData(this.getMenuBaseUrl() + 'doLogin/', connectionData)
            .map((res: Response) => {
                return this.createOperationResult(res);
            })
            .catch(this.handleError);
    }

    getCompaniesForUser(user: string): Observable<any> {
        let obj = { user: user };
        return this.postData(this.getAccountManagerBaseUrl() + 'getCompaniesForUser/', obj)
            .map((res: Response) => {
                return res.json();
            })
            .catch(this.handleError);
    }

    isActivated(application: string, functionality: string): Observable<any> {
        let obj = { application: application, functionality: functionality };
        return this.postData(this.getAccountManagerBaseUrl() + 'isActivated/', obj)
            .map((res: Response) => {
                return res.json();
            })
            .catch(this.handleError);
    }

    loginCompact(connectionData: LoginSession): Observable<OperationResult> {
        return this.postData(this.getAccountManagerBaseUrl() + '/login-compact/', connectionData)
            .map((res: Response) => {
                return res.json();
            })
            .catch(this.handleError);
    }

    logoff(): Observable<OperationResult> {
        let token = this.cookieService.get('authtoken');
        this.logger.debug('httpService.logout (' + token + ')');

        return this.postData(this.getAccountManagerBaseUrl() + 'logoff/', token)
            .map((res: Response) => {
                return res.json();
            })
            .catch(this.handleError);
    }

    logout(): Observable<OperationResult> {
        let token = this.cookieService.get('authtoken');
        this.logger.debug('httpService.logout (' + token + ')');
        return this.postData(this.getMenuBaseUrl() + 'doLogoff/', token)
            .map((res: Response) => {
                return this.createOperationResult(res);
            })
            .catch(this.handleError);
    }

    runDocument(ns: String, args: string = ''): void {
        let subs = this.postData(this.getMenuBaseUrl() + 'runDocument/', { ns: ns, sKeyArgs: args })
            .subscribe(() => {
                subs.unsubscribe();
            });
    }
    runReport(ns: String): Observable<any> {
        return this.postData(this.getMenuBaseUrl() + 'runReport/', { ns: ns })
            .map((res: Response) => {
                return res.json();
            })
            .catch(this.handleError);
    }

    postData(url: string, data: Object): Observable<Response> {
        let headers = new Headers();
        headers.append('Content-Type', 'application/x-www-form-urlencoded');
        return this.http.post(url, this.utils.serializeData(data), { withCredentials: true, headers: headers });
        //return this.http.post(url, this.utils.serializeData(data), { withCredentials: true });
    }

    getComponentUrl(url: string) {
        if (url[0] === '\\') {
            url = url.substring(1);
        }
        return 'app/htmlforms/' + url + '.js';
    }

    getBaseUrl() {
        return this.urlService.getApiUrl();
    }

    getDocumentBaseUrl() {
        return this.urlService.getApiUrl() + 'tb/document/';
    }

    getMenuBaseUrl() {
        return this.urlService.getApiUrl() + 'tb/menu/';
    }

    getAccountManagerBaseUrl() {
        return this.urlService.getBackendUrl() + 'account-manager/';
    }

    getMenuServiceUrl() {
        return this.urlService.getBackendUrl() + 'menu-service/';
    }

    getEnumsServiceUrl() {
        return this.urlService.getBackendUrl() + 'enums-service/';
    }

    protected handleError(error: any): ErrorObservable {
        // In a real world app, we might use a remote logging infrastructure
        // We'd also dig deeper into the error to get a better message
        let errMsg = (error.message) ? error.message :
            error.status ? `${error.status} - ${error.statusText}` : 'Server error';
        console.error(errMsg);

        return Observable.throw(errMsg);
    }

    getEnumsTable(): Observable<any> {
        return this.http.get(this.getEnumsServiceUrl() + 'getEnumsTable/', { withCredentials: true })
            .map((res: Response) => {
                return res.json();
            })
            .catch(this.handleError);
    }

}
