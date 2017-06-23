import { Injectable } from '@angular/core';
import { ViewModeType } from '../../shared/models/view-mode-type.model';
import { Logger } from './logger.service';
import { EventDataService } from './eventdata.service';
export class DocumentService {
    /**
     * @param {?} logger
     * @param {?} eventData
     */
    constructor(logger, eventData) {
        this.logger = logger;
        this.eventData = eventData;
    }
    /**
     * @param {?} cmpId
     * @return {?}
     */
    init(cmpId) {
        this.mainCmpId = cmpId;
    }
    /**
     * @return {?}
     */
    dispose() {
        delete this.mainCmpId;
    }
    /**
     * @return {?}
     */
    getTitle() {
        let /** @type {?} */ title = '...';
        if (this.eventData.model && this.eventData.model.Title && this.eventData.model.Title.value) {
            title = this.eventData.model.Title.value;
        }
        return title;
    }
    /**
     * @return {?}
     */
    getViewModeType() {
        return ViewModeType.R;
        // let viewModeType = ViewModeType.D;
        // if (this.eventData.model && this.eventData.model.viewModeType) {
        //     viewModeType = this.eventData.model.viewModeType;
        // }
        // return viewModeType;
    }
    /**
     * @return {?}
     */
    close() {
    }
}
DocumentService.decorators = [
    { type: Injectable },
];
/**
 * @nocollapse
 */
DocumentService.ctorParameters = () => [
    { type: Logger, },
    { type: EventDataService, },
];
function DocumentService_tsickle_Closure_declarations() {
    /** @type {?} */
    DocumentService.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    DocumentService.ctorParameters;
    /** @type {?} */
    DocumentService.prototype.mainCmpId;
    /** @type {?} */
    DocumentService.prototype.logger;
    /** @type {?} */
    DocumentService.prototype.eventData;
}
