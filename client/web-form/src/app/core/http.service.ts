import { Injectable } from '@angular/core';
import { Http, Response, Headers } from '@angular/http';
import { Observable } from 'rxjs';

import { CookieService } from 'angular2-cookie/services/cookies.service';

import { environment } from './../../environments/environment';

import { OperationResult } from './operation.result';
import { LoginSession } from './../shared/models/login-session';
import { UtilsService } from './utils.service';

import { Logger } from 'libclient';


@Injectable()
export class HttpService {
    private apiBaseUrl = environment.baseUrl + 'tbloader/api/';
    private baseUrl = environment.baseUrl;
    constructor(
        protected http: Http,
        protected utils: UtilsService,
        protected logger: Logger,
        protected cookieService: CookieService) {
        console.log('HttpService instantiated - ' + Math.round(new Date().getTime() / 1000));
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
        return this.postData(this.baseUrl + 'tb/menu/getInstallationInfo/', {})
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
    getLoginActiveThreads() {
        /*return new Promise(function (resolve, reject) {
         me.http.get(me.getDocumentBaseUrl() + "getLoginActiveThreads/")
         .subscribe(response => {
         if (response.ok) {
         resolve(response.text());
         }
         else
         reject(response.toString());
         });
         });*/
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
        return this.apiBaseUrl;
    }

    getDocumentBaseUrl() {
        return this.apiBaseUrl + 'tb/document/';
    }

    getMenuBaseUrl() {
        let url = this.apiBaseUrl + 'tb/menu/';
        return url;
    }

    getAccountManagerBaseUrl() {
        let url = this.baseUrl + 'account-manager/';
        return url;
    }

    protected handleError(error: any) {
        // In a real world app, we might use a remote logging infrastructure
        // We'd also dig deeper into the error to get a better message
        let errMsg = (error.message) ? error.message :
            error.status ? `${error.status} - ${error.statusText}` : 'Server error';
        console.error(errMsg);
        return Observable.throw(errMsg);
    }

    getEnumsTable(): Observable<any> {
        return this.http.get(this.getDocumentBaseUrl() + 'getEnumsTable/', { withCredentials: true })
            .map((res: Response) => {
                return res.json();
            })
            .catch(this.handleError);
    }

}
