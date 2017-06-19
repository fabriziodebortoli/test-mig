import { Injectable } from '@angular/core';
import { EventDataService } from './eventdata.service';
import { DocumentService } from './document.service';
import { Logger } from './logger.service';
export class ExplorerService extends DocumentService {
    /**
     * @param {?} logger
     * @param {?} eventData
     */
    constructor(logger, eventData) {
        super(logger, eventData);
    }
    /**
     * @param {?} application
     * @return {?}
     */
    setSelectedApplication(application) {
    }
}
ExplorerService.decorators = [
    { type: Injectable },
];
/**
 * @nocollapse
 */
ExplorerService.ctorParameters = () => [
    { type: Logger, },
    { type: EventDataService, },
];
function ExplorerService_tsickle_Closure_declarations() {
    /** @type {?} */
    ExplorerService.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    ExplorerService.ctorParameters;
}
