import { Injectable, EventEmitter } from '@angular/core';
import { Logger } from '../../core/services/logger.service';
export class EventManagerService {
    /**
     * @param {?} logger
     */
    constructor(logger) {
        this.logger = logger;
        this.preferenceLoaded = new EventEmitter();
        this.logger.debug('EventManagerService instantiated - ' + Math.round(new Date().getTime() / 1000));
    }
    /**
     * @return {?}
     */
    emitPreferenceLoaded() {
        this.preferenceLoaded.emit();
    }
}
EventManagerService.decorators = [
    { type: Injectable },
];
/**
 * @nocollapse
 */
EventManagerService.ctorParameters = () => [
    { type: Logger, },
];
function EventManagerService_tsickle_Closure_declarations() {
    /** @type {?} */
    EventManagerService.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    EventManagerService.ctorParameters;
    /** @type {?} */
    EventManagerService.prototype.preferenceLoaded;
    /** @type {?} */
    EventManagerService.prototype.logger;
}
