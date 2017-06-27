import { Injectable } from '@angular/core';
export class Logger {
    constructor() { }
    /**
     * @param {?=} message
     * @param {...?} optionalParams
     * @return {?}
     */
    log(message, ...optionalParams) {
        console.log(message, ...optionalParams);
    }
    /**
     * @param {?=} message
     * @param {...?} optionalParams
     * @return {?}
     */
    info(message, ...optionalParams) {
        console.log(message, ...optionalParams);
    }
    /**
     * @param {?=} message
     * @param {...?} optionalParams
     * @return {?}
     */
    debug(message, ...optionalParams) {
        console.debug(message, ...optionalParams);
    }
    /**
     * @param {?=} message
     * @param {...?} optionalParams
     * @return {?}
     */
    warn(message, ...optionalParams) {
        console.warn(message, ...optionalParams);
    }
    /**
     * @param {?=} message
     * @param {...?} optionalParams
     * @return {?}
     */
    error(message, ...optionalParams) {
        console.error(message, ...optionalParams);
    }
}
Logger.decorators = [
    { type: Injectable },
];
/**
 * @nocollapse
 */
Logger.ctorParameters = () => [];
function Logger_tsickle_Closure_declarations() {
    /** @type {?} */
    Logger.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    Logger.ctorParameters;
}
