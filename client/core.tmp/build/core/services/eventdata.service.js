import { Injectable, EventEmitter } from '@angular/core';
export class EventDataService {
    constructor() {
        this.command = new EventEmitter();
        this.change = new EventEmitter();
        this.openDropdown = new EventEmitter();
        this.openMessageDialog = new EventEmitter();
        this.closeMessageDialog = new EventEmitter();
        this.oldModel = {};
        this.model = {};
        this.activation = {};
        this.buttonsState = {};
        console.log('EventDataService created');
    }
}
EventDataService.decorators = [
    { type: Injectable },
];
/**
 * @nocollapse
 */
EventDataService.ctorParameters = () => [];
function EventDataService_tsickle_Closure_declarations() {
    /** @type {?} */
    EventDataService.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    EventDataService.ctorParameters;
    /** @type {?} */
    EventDataService.prototype.command;
    /** @type {?} */
    EventDataService.prototype.change;
    /** @type {?} */
    EventDataService.prototype.openDropdown;
    /** @type {?} */
    EventDataService.prototype.openMessageDialog;
    /** @type {?} */
    EventDataService.prototype.closeMessageDialog;
    /** @type {?} */
    EventDataService.prototype.oldModel;
    /** @type {?} */
    EventDataService.prototype.model;
    /** @type {?} */
    EventDataService.prototype.activation;
    /** @type {?} */
    EventDataService.prototype.buttonsState;
}
