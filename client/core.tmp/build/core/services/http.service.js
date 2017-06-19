import { Injectable } from '@angular/core';
import { Http, Headers } from '@angular/http';
import { Observable } from 'rxjs/Rx';
import { CookieService } from 'angular2-cookie/services/cookies.service';
import { OperationResult } from '../../shared/models/operation-result.model';
import { UtilsService } from './utils.service';
import { UrlService } from './url.service';
import { Logger } from './logger.service';
export class HttpService {
    /**
     * @param {?} http
     * @param {?} utils
     * @param {?} logger
     * @param {?} urlService
     * @param {?} cookieService
     */
    constructor(http, utils, logger, urlService, cookieService) {
        this.http = http;
        this.utils = utils;
        this.logger = logger;
        this.urlService = urlService;
        this.cookieService = cookieService;
    }
    /**
     * @param {?} res
     * @return {?}
     */
    createOperationResult(res) {
        let /** @type {?} */ jObject = res.ok ? res.json() : null;
        let /** @type {?} */ ok = jObject && jObject.success === true;
        let /** @type {?} */ messages = jObject ? jObject.messages : [];
        return new OperationResult(!ok, messages);
    }
    /**
     * @return {?}
     */
    isLogged() {
        return this.postData(this.getMenuBaseUrl() + 'isLogged/', {})
            .map((res) => {
            return res.ok && res.json().success === true;
        })
            .catch(this.handleError);
    }
    /**
     * @return {?}
     */
    getInstallationInfo() {
        return this.postData(this.urlService.getBackendUrl() + 'tb/menu/getInstallationInfo/', {})
            .map((res) => {
            return res.json();
        })
            .catch(this.handleError);
    }
    /**
     * @param {?} connectionData
     * @return {?}
     */
    login(connectionData) {
        return this.postData(this.getMenuBaseUrl() + 'doLogin/', connectionData)
            .map((res) => {
            return this.createOperationResult(res);
        })
            .catch(this.handleError);
    }
    /**
     * @param {?} user
     * @return {?}
     */
    getCompaniesForUser(user) {
        let /** @type {?} */ obj = { user: user };
        return this.postData(this.getAccountManagerBaseUrl() + 'getCompaniesForUser/', obj)
            .map((res) => {
            return res.json();
        })
            .catch(this.handleError);
    }
    /**
     * @param {?} application
     * @param {?} functionality
     * @return {?}
     */
    isActivated(application, functionality) {
        let /** @type {?} */ obj = { application: application, functionality: functionality };
        return this.postData(this.getAccountManagerBaseUrl() + 'isActivated/', obj)
            .map((res) => {
            return res.json();
        })
            .catch(this.handleError);
    }
    /**
     * @param {?} connectionData
     * @return {?}
     */
    loginCompact(connectionData) {
        return this.postData(this.getAccountManagerBaseUrl() + '/login-compact/', connectionData)
            .map((res) => {
            return res.json();
        })
            .catch(this.handleError);
    }
    /**
     * @return {?}
     */
    logoff() {
        let /** @type {?} */ token = this.cookieService.get('authtoken');
        this.logger.debug('httpService.logout (' + token + ')');
        return this.postData(this.getAccountManagerBaseUrl() + 'logoff/', token)
            .map((res) => {
            return res.json();
        })
            .catch(this.handleError);
    }
    /**
     * @return {?}
     */
    logout() {
        let /** @type {?} */ token = this.cookieService.get('authtoken');
        this.logger.debug('httpService.logout (' + token + ')');
        return this.postData(this.getMenuBaseUrl() + 'doLogoff/', token)
            .map((res) => {
            return this.createOperationResult(res);
        })
            .catch(this.handleError);
    }
    /**
     * @param {?} ns
     * @param {?=} args
     * @return {?}
     */
    runDocument(ns, args = '') {
        let /** @type {?} */ subs = this.postData(this.getMenuBaseUrl() + 'runDocument/', { ns: ns, sKeyArgs: args })
            .subscribe(() => {
            subs.unsubscribe();
        });
    }
    /**
     * @param {?} ns
     * @return {?}
     */
    runReport(ns) {
        return this.postData(this.getMenuBaseUrl() + 'runReport/', { ns: ns })
            .map((res) => {
            return res.json();
        })
            .catch(this.handleError);
    }
    /**
     * @param {?} url
     * @param {?} data
     * @return {?}
     */
    postData(url, data) {
        let /** @type {?} */ headers = new Headers();
        headers.append('Content-Type', 'application/x-www-form-urlencoded');
        return this.http.post(url, this.utils.serializeData(data), { withCredentials: true, headers: headers });
        //return this.http.post(url, this.utils.serializeData(data), { withCredentials: true });
    }
    /**
     * @param {?} url
     * @return {?}
     */
    getComponentUrl(url) {
        if (url[0] === '\\') {
            url = url.substring(1);
        }
        return 'app/htmlforms/' + url + '.js';
    }
    /**
     * @return {?}
     */
    getBaseUrl() {
        return this.urlService.getApiUrl();
    }
    /**
     * @return {?}
     */
    getDocumentBaseUrl() {
        return this.urlService.getApiUrl() + 'tb/document/';
    }
    /**
     * @return {?}
     */
    getMenuBaseUrl() {
        return this.urlService.getApiUrl() + 'tb/menu/';
    }
    /**
     * @return {?}
     */
    getAccountManagerBaseUrl() {
        return this.urlService.getBackendUrl() + 'account-manager/';
    }
    /**
     * @return {?}
     */
    getMenuServiceUrl() {
        return this.urlService.getBackendUrl() + 'menu-service/';
    }
    /**
     * @return {?}
     */
    getEnumsServiceUrl() {
        return this.urlService.getBackendUrl() + 'enums-service/';
    }
    /**
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
    /**
     * @return {?}
     */
    getEnumsTable() {
        return this.http.get(this.getEnumsServiceUrl() + 'getEnumsTable/', { withCredentials: true })
            .map((res) => {
            return res.json();
        })
            .catch(this.handleError);
    }
}
HttpService.decorators = [
    { type: Injectable },
];
/**
 * @nocollapse
 */
HttpService.ctorParameters = () => [
    { type: Http, },
    { type: UtilsService, },
    { type: Logger, },
    { type: UrlService, },
    { type: CookieService, },
];
function HttpService_tsickle_Closure_declarations() {
    /** @type {?} */
    HttpService.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    HttpService.ctorParameters;
    /** @type {?} */
    HttpService.prototype.http;
    /** @type {?} */
    HttpService.prototype.utils;
    /** @type {?} */
    HttpService.prototype.logger;
    /** @type {?} */
    HttpService.prototype.urlService;
    /** @type {?} */
    HttpService.prototype.cookieService;
}
