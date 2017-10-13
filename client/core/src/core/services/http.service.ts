﻿import { Injectable } from '@angular/core';
import { Http, Response, Headers, URLSearchParams } from '@angular/http';
import { Observable } from 'rxjs/Observable';
import { ErrorObservable } from 'rxjs/observable/ErrorObservable';

import { CookieService } from 'ngx-cookie';

import { OperationResult } from './../../shared/models/operation-result.model';
import { LoginSession } from './../../shared/models/login-session.model';
import { LoginCompact } from './../../shared/models/login-compact.model';

import { InfoService } from './info.service';
import { UtilsService } from './utils.service';
import { Logger } from './logger.service';

@Injectable()
export class HttpService {

    constructor(
        public http: Http,
        public utils: UtilsService,
        public logger: Logger,
        public cookieService: CookieService,
        public infoService: InfoService) {
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
        return this.postData(this.infoService.getAccountManagerBaseUrl() + 'isValidToken/', params)
            .map((res: Response) => {
                return res.ok && res.json().success === true;
            });
    }

    login(connectionData: LoginSession): Observable<LoginCompact> {
        return this.postData(this.infoService.getAccountManagerBaseUrl() + 'login-compact/', connectionData)
            .map((res: Response) => {
                return res.json();
            });
    }

    getCompaniesForUser(user: string): Observable<any> {
        let obj = { user: user };
        return this.postData(this.infoService.getAccountManagerBaseUrl() + 'getCompaniesForUser/', obj)
            .map((res: Response) => {
                return res.json();
            });
    }

    isActivated(application: string, functionality: string): Observable<any> {
        let obj = { application: application, functionality: functionality };
        return this.postData(this.infoService.getAccountManagerBaseUrl() + 'isActivated/', obj)
            .map((res: Response) => {
                return res.json();
            });
    }

    logoff(params: { authtoken: string }): Observable<OperationResult> {
        return this.postData(this.infoService.getAccountManagerBaseUrl() + 'logoff/', params)
            .map((res: Response) => {
                return res.json();
            });
    }

    canLogoff(params: { authtoken: string }): Observable<OperationResult> {
        return this.postData(this.infoService.getDocumentBaseUrl() + 'canLogoff/', params)
            .map((res: Response) => {
                return this.createOperationResult(res);
            });
    }

    openTBConnection(params: { authtoken: string, isDesktop: boolean }): Observable<OperationResult> {
        return this.postData(this.infoService.getDocumentBaseUrl() + 'initTBLogin/', params)
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
        return this.postData(this.infoService.getDocumentBaseUrl() + 'doLogoff/', params)
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

    handleError(error: any): ErrorObservable {
        // In a real world app, we might use a remote logging infrastructure
        // We'd also dig deeper into the error to get a better message
        let errMsg = (error.message) ? error.message :
            error.status ? `${error.status} - ${error.statusText}` : 'Server error';
        this.logger.error(errMsg);

        return Observable.throw(errMsg);
    }

    getEnumsTable(): Observable<any> {
        return this.http.get(this.infoService.getEnumsServiceUrl() + 'getEnumsTable/', { withCredentials: true })
            .map((res: Response) => {
                return res.json();
            })
            .catch(this.handleError);
    }

    // tslint:disable-next-line:max-line-length
    getHotlinkData(namespace: string, selectionType: string = 'code', filter: string = '', params: URLSearchParams): Observable<any> {
        // tslint:disable-next-line:max-line-length
        return this.http.get(this.infoService.getDataServiceUrl() + 'getdata/' + namespace + '/' + selectionType + '/' + filter, { search: params, withCredentials: true })
            .map((res: Response) => {
                return res.json();
            })
            .catch(this.handleError);
    }

    getHotlinkSelectionTypes(namespace: string): Observable<any> {
        return this.http.get(this.infoService.getDataServiceUrl() + 'getselections/' + namespace + '/', { withCredentials: true })
            .map((res: Response) => {
                return res.json();
            })
            .catch(this.handleError);
    }


    /**
     * API /loadLocalizedElements
     * 
     * @returns {Observable<any>} loadLocalizedElements
     */
    loadLocalizedElements(): Observable<any> {
        return this.postData(this.infoService.getMenuServiceUrl() + 'getLocalizedElements/', {})
            .map((res: Response) => {
                return res.json();
            });
    };

}
