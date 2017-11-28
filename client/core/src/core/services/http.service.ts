import { Injectable } from '@angular/core';
import { Http, Response, Headers, URLSearchParams } from '@angular/http';

import { Observable, ErrorObservable } from '../../rxjs.imports';

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
        public infoService: InfoService) {
    }

    createOperationResult(res: Response): OperationResult {
        let jObject = res.ok ? res.json() : null;
        let ok = jObject && jObject.success === true;
        let message = jObject && jObject.message ? jObject.message : "";
        let messages = jObject && jObject.messages ? jObject.messages : [];
        messages.push(message);
        let respJson = JSON.parse(res.headers.get('Authorization'));
        let tbLoaderName = null;
        if (respJson)
            tbLoaderName = respJson['tbLoaderName'];
        return new OperationResult(!ok, messages, tbLoaderName);
    }

    isServerUp(): Observable<boolean> {
        return this.postData(this.infoService.getAccountManagerBaseUrl() + 'isServerUp/', {}).map(() => true).catch(this.handleError);
    }

    getTranslations(dictionaryId: string, culture: string): Observable<Array<any>> {
        const headers = new Headers();
        headers.append('Accept', 'application/json');
        const url = '/dictionary/' + culture + '/' + dictionaryId + '.json';
        return this.http.get(url, { withCredentials: true, headers: headers })
            .map((res: Response) => {
                return res.json();
            });
    }

    isLogged(params: { authtoken: string }): Observable<boolean> {
        return this.postData(this.infoService.getAccountManagerBaseUrl() + 'isValidToken/', params)
            .map((res: Response) => {
                if (!res.ok)
                    return false;
                let jObj = res.json();
                if (jObj.culture) {
                    this.infoService.setCulture(jObj.culture);
                    this.infoService.saveCulture();
                }
                return jObj.success === true;
            });
    }

    login(connectionData: LoginSession): Observable<LoginCompact> {
        return this.postData(this.infoService.getAccountManagerBaseUrl() + 'login-compact/', connectionData)
            .map((res: Response) => {
                let jObj = res.json();
                if (jObj.culture) {
                    this.infoService.setCulture(jObj.culture);
                    this.infoService.saveCulture();
                }
                return jObj;
            });
    }


    changePassword(params: { user: string, oldPassword: string, newPassword: string }): Observable<LoginCompact> {
        return this.postData(this.infoService.getAccountManagerBaseUrl() + 'change-password/', params)
            .map((res: Response) => {
                let jObj = res.json();
                return jObj;
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
                let jObj = res.json();
                this.infoService.resetCulture();
                return jObj;
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
            });
    }

    closeTBConnection(params: { authtoken: string }): Observable<OperationResult> {
        return this.postData(this.infoService.getDocumentBaseUrl() + 'doLogoff/', params)
            .map((res: Response) => {
                return this.createOperationResult(res);
            });
    }

    postData(url: string, data: Object): Observable<Response> {
        const headers = new Headers({
            'Content-Type': 'application/json',
            'Authorization': this.infoService.getAuthorization()
        });
        return this.http.post(url, data, { withCredentials: true, headers: headers })
            .catch(this.handleError);
    }

    getData(url: string, data: Object): Observable<Response> {
        const headers = new Headers({
            'Authorization': this.infoService.getAuthorization()
        });
        //qui dobbiamo crare dei params
        //let dat = this.utils.serializeData(data);
        return this.http.get(url, { withCredentials: true, headers: headers, params: data })
            .catch(this.handleError);
    }

    handleError(error: any): ErrorObservable {
        // In a real world app, we might use a remote logging infrastructure
        // We'd also dig deeper into the error to get a better message
        let errMsg = (error.message) ? error.message :
            error.status ? `${error.status} - ${error.statusText}` : 'Server error';
        if (this.logger)
            this.logger.error(errMsg);

        return Observable.throw(errMsg);
    }

    getEnumsTable(): Observable<any> {
        return this.getData(this.infoService.getEnumsServiceUrl() + 'getEnumsTable/', { withCredentials: true })
            .map((res: Response) => {
                return res.json();
            })
            .catch(this.handleError);
    }

    getFormattersTable(): Observable<any> {
        return this.getData(this.infoService.getFormattersServiceUrl() + 'getFormattersTable/', { withCredentials: true })
            .map((res: Response) => {
                return res.json();
            })
            .catch(this.handleError);
    }

    // tslint:disable-next-line:max-line-length
    getHotlinkData(namespace: string, selectionType: string = 'code', params: URLSearchParams): Observable<any> {
        let headers = new Headers();
        headers.append('Authorization', this.infoService.getAuthorization());

        return this.http.get(this.infoService.getDataServiceUrl() + 'getdata/' + namespace + '/' + selectionType, { search: params, withCredentials: true, headers: headers })
            .map((res: Response) => {
                return res.json();
            })
            .catch(this.handleError);
    }

    getHotlinkSelectionTypes(namespace: string): Observable<any> {
        let headers = new Headers();
        headers.append('Authorization', this.infoService.getAuthorization());

        return this.http.get(this.infoService.getDataServiceUrl() + 'getselections/' + namespace + '/', { withCredentials: true, headers: headers })
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

    /**
     * API /getPreferences
     * 
     * @returns {Observable<any>} getPreferences
     */
    getPreferences(): Observable<any> {
        let urlToRun = this.infoService.getMenuServiceUrl() + 'getPreferences/';
        let obj = { user: localStorage.getItem('_user'), company: localStorage.getItem('_company') }

        return this.postData(urlToRun, obj)
            .map((res: any) => {
                return res.json();
            })
            .catch(this.handleError);
    }

    /**
     * API /setPreference
     * 
     * @param {string} referenceName
     * @param {string} referenceValue
     * 
     * @returns {Observable<any>} setPreference
     */
    setPreference(referenceName: string, referenceValue: string): Observable<any> {
        let obj = { name: referenceName, value: referenceValue, user: localStorage.getItem('_user'), company: localStorage.getItem('_company') };
        var urlToRun = this.infoService.getMenuServiceUrl() + 'setPreference/';
        return this.postData(urlToRun, obj)
            .map((res: Response) => {
                return res.ok;
            });
    }

    /**
    * API /getThemedSettings
    * 
    * @returns {Observable<any>} getThemedSettings
    */
    getThemedSettings(): Observable<any> {
        let obj = { authtoken: sessionStorage.getItem('authtoken') };
        var urlToRun = this.infoService.getMenuServiceUrl() + 'getThemedSettings/';
        return this.postData(urlToRun, obj)
            .map((res: Response) => {
                return res.json();
            });
    }

    getHotlinkTestData(page: number, rows: number): Observable<any> {
        let headers = new Headers();
        headers.append('Authorization', this.infoService.getAuthorization());
        return this.http.get('http://localhost:50419/api/hotlink/' + page + '/' + rows, { withCredentials: true, headers: headers })
            .map((res: Response) => {
                return res.json();
            })
            .catch(this.handleError);
    }

}
