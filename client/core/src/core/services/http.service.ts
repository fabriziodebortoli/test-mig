import { AppConfigService } from './app-config.service';
import { LoginCompact } from './../../shared/models/login-compact.model';
import { Injectable } from '@angular/core';
import { Http, Response, Headers, URLSearchParams } from '@angular/http';

import { Observable } from 'rxjs/Rx';
import { ErrorObservable } from 'rxjs/observable/ErrorObservable';

import { CookieService } from 'angular2-cookie/services/cookies.service';

import { LoginSession, OperationResult } from '../../shared/models';

import { UtilsService } from './utils.service';
import { UrlService } from './url.service';
import { Logger } from './logger.service';

@Injectable()
export class HttpService {

    constructor(
        protected http: Http,
        protected utils: UtilsService,
        protected logger: Logger,
        protected urlService: UrlService,
        protected cookieService: CookieService,
        private appConfigService: AppConfigService) {
    }

    createOperationResult(res: Response): OperationResult {
        let jObject = res.ok ? res.json() : null;
        let ok = jObject && jObject.success === true;
        let message = jObject && jObject.message ? jObject.message : "";
        let messages = jObject && jObject.messages ? jObject.messages : [];
        messages.push(message);
        return new OperationResult(!ok, messages);
    }

    isLogged(params: { authtoken: string }): Observable<boolean> {
        return this.postData(this.getAccountManagerBaseUrl() + 'isValidToken/', params)
            .map((res: Response) => {
                return res.ok && res.json().success === true;
            });
    }

    login(connectionData: LoginSession): Observable<LoginCompact> {
        return this.postData(this.getAccountManagerBaseUrl() + 'login-compact/', connectionData)
            .map((res: Response) => {
                return res.json();
            });
    }

    getCompaniesForUser(user: string): Observable<any> {
        let obj = { user: user };
        return this.postData(this.getAccountManagerBaseUrl() + 'getCompaniesForUser/', obj)
            .map((res: Response) => {
                return res.json();
            });
    }

    isActivated(application: string, functionality: string): Observable<any> {
        let obj = { application: application, functionality: functionality };
        return this.postData(this.getAccountManagerBaseUrl() + 'isActivated/', obj)
            .map((res: Response) => {
                return res.json();
            });
    }

    logoff(params: { authtoken: string }): Observable<OperationResult> {
        return this.postData(this.getAccountManagerBaseUrl() + 'logoff/', params)
            .map((res: Response) => {
                return res.json();
            });
    }

    openTBConnection(params: { authtoken: string }): Observable<OperationResult> {
        return this.postData(this.getDocumentBaseUrl() + 'initTBLogin/', params)
            .map((res: Response) => {
                return this.createOperationResult(res);
            })
    }

    postDataWithAllowOrigin(url: string): Observable<OperationResult> {
        let token = this.cookieService.get('authtoken');
        let headers = new Headers();
        headers.append('Access-Control-Allow-Origin', window.location.origin);
        headers.append('Access-Control-Allow-Headers', 'Access-Control-Allow-Origin');
        return this.http.post(url, undefined, { withCredentials: true, headers: headers })
            .map((res: Response) => {
                return this.createOperationResult(res);
            });
    }

    closeTBConnection(params: { authtoken: string }): Observable<OperationResult> {
        return this.postData(this.getDocumentBaseUrl() + 'doLogoff/', params)
            .map((res: Response) => {
                return this.createOperationResult(res);
            });
    }

    postData(url: string, data: Object): Observable<Response> {
        let headers = new Headers();
        headers.append('Content-Type', 'application/x-www-form-urlencoded');
        return this.http.post(url, this.utils.serializeData(data), { withCredentials: true, headers: headers }).catch(this.handleError);
        //return this.http.post(url, this.utils.serializeData(data), { withCredentials: true });
    }
    /**
   * API /getProductInfo
   * 
   * @returns {Observable<any>} getProductInfo
   */
    getProductInfo(): Observable<any> {
        let obj = { token: this.cookieService.get('authtoken') }
        return this.postData(this.getDocumentBaseUrl() + 'getProductInfo/', obj)
            .map((res: Response) => {
                return res.json();
            });
    }
    getDictionaries(): Observable<any> {
        let obj = {}
        return this.postData(this.getDataServiceUrl() + 'getinstalleddictionaries', obj)
            .map((res: Response) => {
                return res.json();
            });
    }
    getBaseUrl() {
        return this.urlService.getApiUrl();
    }

    getDocumentBaseUrl() {
        let url = this.appConfigService.config.isDesktop ? 'http://localhost/' : this.urlService.getApiUrl()
        return url + 'tb/document/';
    }

    getMenuBaseUrl() {
        let url = this.appConfigService.config.isDesktop ? 'http://localhost/' : this.urlService.getApiUrl()
        return url + 'tb/menu/';
    }

    getAccountManagerBaseUrl() {
        return this.urlService.getBackendUrl() + '/account-manager/';
    }

    getMenuServiceUrl() {
        return this.urlService.getBackendUrl() + '/menu-service/';
    }

    getEnumsServiceUrl() {
        let url = this.urlService.getBackendUrl() + '/enums-service/';
        return url;
    }
    getDataServiceUrl() {
        let url = this.urlService.getBackendUrl() + '/data-service/';
        return url;
    }

    getReportServiceUrl() {
        let url = this.urlService.getBackendUrl() + '/rs/';
        return url;
    }

    handleError(error: any): ErrorObservable {
        // In a real world app, we might use a remote logging infrastructure
        // We'd also dig deeper into the error to get a better message
        let errMsg = (error.message) ? error.message :
            error.status ? `${error.status} - ${error.statusText}` : 'Server error';
        this.logger.error(errMsg);

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
