import { Injectable } from '@angular/core';
import { Http } from '@angular/http';
import { EventDataService } from './eventdata.service';
import { DocumentService } from './document.service';
import { UrlService } from './url.service';
import { Logger } from './logger.service';
export class DataService extends DocumentService {
    /**
     * @param {?} logger
     * @param {?} eventData
     * @param {?} http
     * @param {?} urlService
     */
    constructor(logger, eventData, http, urlService) {
        super(logger, eventData);
        this.http = http;
        this.urlService = urlService;
    }
    /**
     * @param {?} nameSpace
     * @param {?} selectionType
     * @param {?} params
     * @return {?}
     */
    getData(nameSpace, selectionType, params) {
        let /** @type {?} */ url = this.urlService.getBackendUrl() + 'data-service/getdata/' + nameSpace + '/' + selectionType;
        return this.http.get(url, { search: params, withCredentials: true }).map((res) => res.json());
    }
    /**
     * @param {?} nameSpace
     * @param {?} selectionType
     * @return {?}
     */
    getColumns(nameSpace, selectionType) {
        let /** @type {?} */ url = this.urlService.getBackendUrl() + 'data-service/getcolumns/' + nameSpace + '/' + selectionType;
        return this.http.get(url, { withCredentials: true }).map((res) => res.json());
    }
    /**
     * @param {?} nameSpace
     * @return {?}
     */
    getSelections(nameSpace) {
        let /** @type {?} */ url = this.urlService.getBackendUrl() + 'data-service/getselections/' + nameSpace;
        return this.http.get(url, { withCredentials: true }).map((res) => res.json());
    }
    /**
     * @param {?} nameSpace
     * @return {?}
     */
    getParameters(nameSpace) {
        let /** @type {?} */ url = this.urlService.getBackendUrl() + 'data-service/getparameters/' + nameSpace;
        return this.http.get(url, { withCredentials: true }).map((res) => res.json());
    }
}
DataService.decorators = [
    { type: Injectable },
];
/**
 * @nocollapse
 */
DataService.ctorParameters = () => [
    { type: Logger, },
    { type: EventDataService, },
    { type: Http, },
    { type: UrlService, },
];
function DataService_tsickle_Closure_declarations() {
    /** @type {?} */
    DataService.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    DataService.ctorParameters;
    /** @type {?} */
    DataService.prototype.http;
    /** @type {?} */
    DataService.prototype.urlService;
}
