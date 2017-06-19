import { Injectable } from '@angular/core';
export class UrlService {
    constructor() {
        this.port = 5000;
        this.secure = false;
    }
    /**
     * @return {?}
     */
    getBackendUrl() {
        if (!this.host) {
            this.host = window.location.host;
        }
        let /** @type {?} */ protocol = 'http:';
        if (this.secure) {
            protocol = 'https:';
        }
        return protocol += '//' + this.host + ':' + this.port;
    }
    /**
     * @return {?}
     */
    getApiUrl() {
        return this.getBackendUrl() + '/tbloader/api/';
    }
    /**
     * @return {?}
     */
    getWsUrl() {
        if (!this.host) {
            this.host = window.location.host;
        }
        let /** @type {?} */ protocol = 'ws:';
        if (this.secure) {
            protocol = 'wss:';
        }
        return protocol += '//' + this.host + ':' + this.port;
    }
    /**
     * @param {?} port
     * @return {?}
     */
    setPort(port) {
        this.port = port;
    }
    /**
     * @param {?} host
     * @return {?}
     */
    setHost(host) {
        this.host = host;
    }
    /**
     * @param {?} secure
     * @return {?}
     */
    setSecure(secure) {
        this.secure = secure;
    }
}
UrlService.decorators = [
    { type: Injectable },
];
/**
 * @nocollapse
 */
UrlService.ctorParameters = () => [];
function UrlService_tsickle_Closure_declarations() {
    /** @type {?} */
    UrlService.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    UrlService.ctorParameters;
    /** @type {?} */
    UrlService.prototype.host;
    /** @type {?} */
    UrlService.prototype.port;
    /** @type {?} */
    UrlService.prototype.secure;
}
