import { Injectable } from '@angular/core';
import { Http, Response } from '@angular/http';
import { Observable } from 'rxjs';

import { Logger } from 'libclient';

import { CookieService } from 'angular2-cookie/services/cookies.service';
import { UtilsService, HttpService } from 'tb-core';

import { environment } from '../../../environments/environment';

@Injectable()
export class HttpMenuService {

    private baseUrl = environment.apiBaseUrl + 'tb/menu/';

    constructor(
        private http: Http,
        private utilsService: UtilsService,
        private logger: Logger,
        private cookieService: CookieService) {
        this.logger.debug('HttpMenuService instantiated - ' + Math.round(new Date().getTime() / 1000));
    }

    /**
     * API /getMenuElements
     * 
     * @returns {Observable<any>} getMenuElements
     */
    getMenuElements(): Observable<any> {
        return this.http.get(this.baseUrl + 'getMenuElements/', { withCredentials: true })
            .map((res: Response) => {
                return res.json();
            })
            .catch(this.handleError);
    }

    /**
     * API /getProductInfo
     * 
     * @returns {Observable<any>} getProductInfo
     */
    getProductInfo(): Observable<any> {
        return this.http.get(this.baseUrl + 'getProductInfo/', { withCredentials: true })
            .map((res: Response) => {
                return res.json();
            })
            .catch(this.handleError);
    }

    /**
     * API /getPreferences
     * 
     * @returns {Observable<any>} getPreferences
     */
    getPreferences(): Observable<any> {
        return this.http.get(this.baseUrl + 'getPreferences/', { withCredentials: true })
            .map((res: Response) => {
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
        let obj = { name: referenceName, value: referenceValue };
        var urlToRun = this.baseUrl + 'setPreference/';
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
        return this.http.get(this.baseUrl + 'getThemedSettings/', { withCredentials: true })
            .map((res: Response) => {
                return res.json();
            })
            .catch(this.handleError);
    }

    /**
     * API /getConnectionInfo
     * 
     * @returns {Observable<any>} getConnectionInfo
     */
    getConnectionInfo(): Observable<any> {
        return this.http.get(this.baseUrl + 'getConnectionInfo/', { withCredentials: true })
            .map((res: Response) => {
                return res.json();
            })
            .catch(this.handleError);
    }

    /**
     * API /activateViaSMS
     * 
     * @returns {Observable<any>} activateViaSMS
     */
    activateViaSMS() {
        var urlToRun = this.baseUrl + 'activateViaSMS/';
        let subs = this.postData(urlToRun, undefined)
            .map((res: Response) => {
                return res.ok;
            })
            .subscribe(result => {
                subs.unsubscribe();
            });
    }

    /**
     * API /goToSite
     * 
     * @returns {Observable<any>} goToSite
     */
    goToSite() {
        var urlToRun = this.baseUrl + 'producerSite/';
        let subs = this.postData(urlToRun, undefined)
            .map((res: Response) => {
                return res.ok;
            })
            .subscribe(result => {
                subs.unsubscribe();
            });
    }

    /**
     * API /clearCachedData
     * 
     * @returns {Observable<any>} clearCachedData
     */
    clearCachedData(): Observable<any> {
        var urlToRun = this.baseUrl + 'clearCachedData/';
        return this.postData(urlToRun, undefined)
            .map((res: Response) => {
                return res.ok;
            });
    }

    /**
     * API /activateViaInternet
     * 
     * @returns {Observable<any>} activateViaInternet
     */
    activateViaInternet() {
        var urlToRun = this.baseUrl + 'activateViaInternet/';
        let subs = this.postData(urlToRun, undefined)
            .map((res: Response) => {
                return res.ok;
            })
            .subscribe(result => {
                subs.unsubscribe();
            });
    }

    /**
     * API /favoriteObject
     * 
     * @returns {Observable<any>} favoriteObject
     */
    favoriteObject(object) {
        let obj = { target: object.target, objectType: object.objectType, objectName: object.objectName };
        
        var urlToRun = this.baseUrl + 'favoriteObject/';
        let subs = this.postData(urlToRun, obj)
            .map((res: Response) => {
                return res.ok;
            })
            .subscribe(result => {
                subs.unsubscribe();
            });
    }

    /**
     * API /unFavoriteObject
     * 
     * @returns {Observable<any>} unFavoriteObject
     */
    unFavoriteObject(object) {
        let obj = { target: object.target, objectType: object.objectType, objectName: object.objectName };
        var urlToRun = this.baseUrl + 'unFavoriteObject/';
        let subs = this.postData(urlToRun, obj)
            .map((res: Response) => {
                return res.ok;
            })
            .subscribe(result => {
                subs.unsubscribe();
            });
    }

    /**
     * API /mostUsedClearAll
     *
     * @returns {Observable<any>} mostUsedClearAll
     */
    mostUsedClearAll(): Observable<any> {
        return this.postData(this.baseUrl + 'clearAllMostUsed/', undefined)
            .map((res: Response) => {
                return res.ok;
            });

    };

    /**
     * API /getMostUsedShowNr
     * 
     * @returns {Observable<any>} getMostUsedShowNr
     */
    getMostUsedShowNr(callback) {

        var urlToRun = this.baseUrl + 'getMostUsedShowNr/';
        let subs = this.postData(urlToRun, undefined)
            .map((res: Response) => {
                callback(res);
                return res.ok;
            })
            .catch(this.handleError)
            .subscribe(result => {
                subs.unsubscribe();
            });
    }

    /**
     * API /addToMostUsed
     * 
     * @returns {Observable<any>} addToMostUsed
     */
    addToMostUsed(object): Observable<any> {
        let obj = { target: object.target, objectType: object.objectType, objectName: object.objectName };
        return this.postData(this.baseUrl + 'addToMostUsed/', obj)
            .map((res: Response) => {
                return res.ok;
            });
    };

    /**
     * API /removeFromMostUsed
     * 
     * @returns {Observable<any>} removeFromMostUsed
     */
    removeFromMostUsed = function (object) {
        let obj = { target: object.target, objectType: object.objectType, objectName: object.objectName };
        return this.postData(this.baseUrl + 'removeFromMostUsed/', obj)
            .map((res: Response) => {
                return res.ok;
            });
    };

    /**
     * API /loadLocalizedElements
     * 
     * @returns {Observable<any>} loadLocalizedElements
     */
    loadLocalizedElements(needLoginThread): Observable<any> {
        return this.http.get(this.baseUrl + 'getLocalizedElements/?needLoginThread=' + needLoginThread, { withCredentials: true })
            .map((res: Response) => {
                return res.json();
            })
            .catch(this.handleError);
    };

    private postData(url: string, data: Object): Observable<Response> {
        return this.http.post(url, this.utilsService.serializeData(data), { withCredentials: true })
            .catch(this.handleError);
    }

    /**
     * TODO refactor with custom logger
     */
    private handleError(error: any) {
        // In a real world app, we might use a remote logging infrastructure
        // We'd also dig deeper into the error to get a better message
        let errMsg = (error.message) ? error.message :
            error.status ? `${error.status} - ${error.statusText}` : 'Server error';
        console.error(errMsg);
        return Observable.throw(errMsg);
    }
}