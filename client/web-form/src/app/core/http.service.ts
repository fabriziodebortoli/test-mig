import { Injectable } from '@angular/core';
import { Http, Response, Headers, URLSearchParams } from '@angular/http';
import { Observable } from 'rxjs/Rx';

import { CookieService } from 'angular2-cookie/services/cookies.service';

import { environment } from './../../environments/environment';

import { OperationResult } from './operation.result';
import { LoginSession } from './../shared/models/login-session';
import { UtilsService } from './utils.service';

import { Logger } from './logger.service';


@Injectable()
export class HttpService {
    private apiBaseUrl = environment.baseUrl + 'tbloader/api/';
    private baseUrl = environment.baseUrl;
    constructor(
        protected http: Http,
        protected utils: UtilsService,
        protected logger: Logger,
        protected cookieService: CookieService) {
    }
    createOperationResult(res: Response): OperationResult {
        let jObject = res.ok ? res.json() : null;
        let ok = jObject && jObject.success === true;
        let messages = jObject ? jObject.messages : [];
        return new OperationResult(!ok, messages);
    }
    isLogged(): Observable<boolean> {
        let obj = { authtoken: this.cookieService.get('authtoken') };
        return this.postData(this.getAccountManagerBaseUrl() + 'isValidToken/', obj)
            .map((res: Response) => {
                return res.ok && res.json().success === true;
            })
            .catch(this.handleError);
    }

    login(connectionData: LoginSession): Observable<OperationResult> {
        return this.postData(this.getAccountManagerBaseUrl() + 'login-compact/', connectionData)
            .map((res: Response) => {
                this.cookieService.put('authtoken', res.ok ? res.json().authtoken : null);
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

    logoff(): Observable<OperationResult> {
        let token = this.cookieService.get('authtoken');
        this.logger.debug('httpService.logout (' + token + ')');

        return this.postData(this.getAccountManagerBaseUrl() + 'logoff/', token)
            .map((res: Response) => {
                return this.createOperationResult(res);
            })
            .catch(this.handleError);
    }
    getDBTData(): Observable<any[]> {
        const obj = { };
        return this.postData(this.getDocumentBaseUrl() + 'getDBTData/', obj)
            .map((res: Response) => {
                return res.json();
            })
            .catch(this.handleError);
    }
    openTBConnection(): Observable<OperationResult> {
        let token = this.cookieService.get('authtoken');
        return this.postData(this.getMenuBaseUrl() + 'initTBLogin/', token)
            .map((res: Response) => {
                return this.createOperationResult(res);
            })
            .catch(this.handleError);
    }
    closeTBConnection(): Observable<OperationResult> {
        let token = this.cookieService.get('authtoken');
        this.logger.debug('httpService.logout (' + token + ')');
        return this.postData(this.getMenuBaseUrl() + 'doLogoff/', token)
            .map((res: Response) => {
                return this.createOperationResult(res);
            })
            .catch(this.handleError);
    }

    postData(url: string, data: Object): Observable<Response> {
        let headers = new Headers();
        headers.append('Content-Type', 'application/x-www-form-urlencoded');
        return this.http.post(url, this.utils.serializeData(data), { withCredentials: true, headers: headers });
        //return this.http.post(url, this.utils.serializeData(data), { withCredentials: true });
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

    getMenuServiceUrl() {
        let url = this.baseUrl + 'menu-service/';
        return url;
    }

    getEnumsServiceUrl() {
        let url = this.baseUrl + 'enums-service/';
        return url;
    }
    getDataServiceUrl() {
        let url = this.baseUrl + 'data-service/';
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
        return this.http.get(this.getEnumsServiceUrl() + 'getEnumsTable/', { withCredentials: true })
            .map((res: Response) => {
                return res.json();
            })
            .catch(this.handleError);
    }

    // tslint:disable-next-line:max-line-length
    getHotlinkData(namespace: string, selectionType: string = 'code', filter: string = '', params: URLSearchParams): Observable<any> {
        // tslint:disable-next-line:max-line-length
        return this.http.get(this.getDataServiceUrl() + 'getdata/' + namespace + '/' + selectionType + '/' + filter, { search: params, withCredentials: true })
            .map((res: Response) => {
                return res.json();
            })
            .catch(this.handleError);
    }

    getHotlinkSelectionTypes(namespace: string): Observable<any> {
        return this.http.get(this.getDataServiceUrl() + 'getselections/' + namespace + '/', { withCredentials: true })
            .map((res: Response) => {
                return res.json();
            })
            .catch(this.handleError);
    }

}
