import { Injectable } from '@angular/core';
import { Http, Headers } from '@angular/http';
import { Observable } from 'rxjs/Rx';
import { UtilsService } from '../../core/services/utils.service';
import { HttpService } from '../../core/services/http.service';
import { Logger } from '../../core/services/logger.service';
import { CookieService } from 'angular2-cookie/services/cookies.service';
export class HttpMenuService {
    /**
     * @param {?} http
     * @param {?} utilsService
     * @param {?} logger
     * @param {?} cookieService
     * @param {?} httpService
     */
    constructor(http, utilsService, logger, cookieService, httpService) {
        this.http = http;
        this.utilsService = utilsService;
        this.logger = logger;
        this.cookieService = cookieService;
        this.httpService = httpService;
        /**
         * API /removeFromMostUsed
        
        \@returns {Observable<any>} removeFromMostUsed
         */
        this.removeFromMostUsed = function (object) {
            let obj = { target: object.target, objectType: object.objectType, objectName: object.objectName, user: this.cookieService.get('_user'), company: this.cookieService.get('_company') };
            return this.postData(this.httpService.getMenuServiceUrl() + 'removeFromMostUsed/', obj)
                .map((res) => {
                return res.ok;
            });
        };
    }
    /**
     * @param {?} url
     * @param {?} data
     * @return {?}
     */
    postData(url, data) {
        let /** @type {?} */ headers = new Headers();
        headers.append('Content-Type', 'application/x-www-form-urlencoded');
        return this.http.post(url, this.utilsService.serializeData(data), { withCredentials: true, headers: headers });
    }
    /**
     * API /getMenuElements
    
    \@returns {Observable<any>} getMenuElements
     * @return {?}
     */
    getMenuElements() {
        let /** @type {?} */ obj = { user: this.cookieService.get('_user'), company: this.cookieService.get('_company'), token: this.cookieService.get('authtoken') };
        let /** @type {?} */ urlToRun = this.httpService.getMenuServiceUrl() + 'getMenuElements/';
        return this.postData(urlToRun, obj)
            .map((res) => {
            return res.json();
        })
            .catch(this.handleError);
    }
    /**
     * API /getPreferences
    
    \@returns {Observable<any>} getPreferences
     * @return {?}
     */
    getPreferences() {
        let /** @type {?} */ urlToRun = this.httpService.getMenuServiceUrl() + 'getPreferences/';
        let /** @type {?} */ obj = { user: this.cookieService.get('_user'), company: this.cookieService.get('_company') };
        return this.postData(urlToRun, obj)
            .map((res) => {
            return res.json();
        })
            .catch(this.handleError);
    }
    /**
     * API /setPreference
    
    \@param {string} referenceName
    \@param {string} referenceValue
    
    \@returns {Observable<any>} setPreference
     * @param {?} referenceName
     * @param {?} referenceValue
     * @return {?}
     */
    setPreference(referenceName, referenceValue) {
        let /** @type {?} */ obj = { name: referenceName, value: referenceValue, user: this.cookieService.get('_user'), company: this.cookieService.get('_company') };
        var /** @type {?} */ urlToRun = this.httpService.getMenuServiceUrl() + 'setPreference/';
        return this.postData(urlToRun, obj)
            .map((res) => {
            return res.ok;
        });
    }
    /**
     * API /getThemedSettings
    
    \@returns {Observable<any>} getThemedSettings
     * @return {?}
     */
    getThemedSettings() {
        let /** @type {?} */ obj = { token: this.cookieService.get('authtoken') };
        var /** @type {?} */ urlToRun = this.httpService.getMenuServiceUrl() + 'getThemedSettings/';
        return this.postData(urlToRun, obj)
            .map((res) => {
            return res.json();
        });
    }
    /**
     * API /getConnectionInfo
    
    \@returns {Observable<any>} getConnectionInfo
     * @return {?}
     */
    getConnectionInfo() {
        let /** @type {?} */ obj = { token: this.cookieService.get('authtoken') };
        var /** @type {?} */ urlToRun = this.httpService.getMenuServiceUrl() + 'getConnectionInfo/';
        return this.postData(urlToRun, obj)
            .map((res) => {
            return res.json();
        });
    }
    /**
     * API /favoriteObject
    
    \@returns {Observable<any>} favoriteObject
     * @param {?} object
     * @return {?}
     */
    favoriteObject(object) {
        let /** @type {?} */ obj = { target: object.target, objectType: object.objectType, objectName: object.objectName, user: this.cookieService.get('_user'), company: this.cookieService.get('_company') };
        var /** @type {?} */ urlToRun = this.httpService.getMenuServiceUrl() + 'favoriteObject/';
        let /** @type {?} */ subs = this.postData(urlToRun, obj)
            .map((res) => {
            return res.ok;
        })
            .subscribe(result => {
            subs.unsubscribe();
        });
    }
    /**
     * API /unFavoriteObject
    
    \@returns {Observable<any>} unFavoriteObject
     * @param {?} object
     * @return {?}
     */
    unFavoriteObject(object) {
        let /** @type {?} */ obj = { target: object.target, objectType: object.objectType, objectName: object.objectName, user: this.cookieService.get('_user'), company: this.cookieService.get('_company') };
        var /** @type {?} */ urlToRun = this.httpService.getMenuServiceUrl() + 'unFavoriteObject/';
        let /** @type {?} */ subs = this.postData(urlToRun, obj)
            .map((res) => {
            return res.ok;
        })
            .subscribe(result => {
            subs.unsubscribe();
        });
    }
    /**
     * API /mostUsedClearAll
    
    \@returns {Observable<any>} mostUsedClearAll
     * @return {?}
     */
    mostUsedClearAll() {
        let /** @type {?} */ obj = { user: this.cookieService.get('_user'), company: this.cookieService.get('_company') };
        return this.postData(this.httpService.getMenuServiceUrl() + 'clearAllMostUsed/', obj)
            .map((res) => {
            return res.ok;
        });
    }
    ;
    /**
     * API /getMostUsedShowNr
    
    \@returns {Observable<any>} getMostUsedShowNr
     * @param {?} callback
     * @return {?}
     */
    getMostUsedShowNr(callback) {
        let /** @type {?} */ obj = { user: this.cookieService.get('_user'), company: this.cookieService.get('_company') };
        return this.postData(this.httpService.getMenuServiceUrl() + 'getMostUsedShowNr/', obj)
            .map((res) => {
            return res.json();
        });
    }
    /**
     * API /addToMostUsed
    
    \@returns {Observable<any>} addToMostUsed
     * @param {?} object
     * @return {?}
     */
    addToMostUsed(object) {
        let /** @type {?} */ obj = { target: object.target, objectType: object.objectType, objectName: object.objectName, user: this.cookieService.get('_user'), company: this.cookieService.get('_company') };
        return this.postData(this.httpService.getMenuServiceUrl() + 'addToMostUsed/', obj)
            .map((res) => {
            return res.ok;
        });
    }
    ;
    /**
     * API /clearCachedData
    
    \@returns {Observable<any>} clearCachedData
     * @return {?}
     */
    clearCachedData() {
        var /** @type {?} */ urlToRun = this.httpService.getMenuServiceUrl() + 'clearCachedData/';
        return this.postData(urlToRun, undefined)
            .map((res) => {
            return res.ok;
        });
    }
    /**
     * API /loadLocalizedElements
    
    \@returns {Observable<any>} loadLocalizedElements
     * @param {?} needLoginThread
     * @return {?}
     */
    loadLocalizedElements(needLoginThread) {
        let /** @type {?} */ obj = { token: this.cookieService.get('authtoken') };
        return this.postData(this.httpService.getMenuServiceUrl() + 'getLocalizedElements/', obj)
            .map((res) => {
            return res.json();
        });
    }
    ;
    /**
     * API /getProductInfo
    
    \@returns {Observable<any>} getProductInfo
     * @return {?}
     */
    getProductInfo() {
        let /** @type {?} */ obj = { token: this.cookieService.get('authtoken') };
        return this.postData(this.httpService.getMenuServiceUrl() + 'getProductInfo/', obj)
            .map((res) => {
            return res.json();
        });
    }
    /**
     * API /activateViaSMS
    
    \@returns {Observable<any>} activateViaSMS
     * @return {?}
     */
    activateViaSMS() {
        return this.http.get(this.httpService.getMenuServiceUrl() + 'getPingViaSMSUrl/', { withCredentials: true })
            .map((res) => {
            return res.json();
        });
    }
    /**
     * API /goToSite
    
    \@returns {Observable<any>} goToSite
     * @return {?}
     */
    goToSite() {
        return this.http.get(this.httpService.getMenuServiceUrl() + 'getProducerSite/', { withCredentials: true })
            .map((res) => {
            return res.json();
        });
    }
    /**
     * TODO refactor with custom logger
     * @param {?} error
     * @return {?}
     */
    handleError(error) {
        // In a real world app, we might use a remote logging infrastructure
        // We'd also dig deeper into the error to get a better message
        let /** @type {?} */ errMsg = (error.message) ? error.message :
            error.status ? `${error.status} - ${error.statusText}` : 'Server error';
        console.error(errMsg);
        return Observable.throw(errMsg);
    }
}
HttpMenuService.decorators = [
    { type: Injectable },
];
/**
 * @nocollapse
 */
HttpMenuService.ctorParameters = () => [
    { type: Http, },
    { type: UtilsService, },
    { type: Logger, },
    { type: CookieService, },
    { type: HttpService, },
];
function HttpMenuService_tsickle_Closure_declarations() {
    /** @type {?} */
    HttpMenuService.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    HttpMenuService.ctorParameters;
    /**
     * API /removeFromMostUsed
    
    \@returns {Observable<any>} removeFromMostUsed
     * @type {?}
     */
    HttpMenuService.prototype.removeFromMostUsed;
    /** @type {?} */
    HttpMenuService.prototype.http;
    /** @type {?} */
    HttpMenuService.prototype.utilsService;
    /** @type {?} */
    HttpMenuService.prototype.logger;
    /** @type {?} */
    HttpMenuService.prototype.cookieService;
    /** @type {?} */
    HttpMenuService.prototype.httpService;
}
