import { Injectable, EventEmitter } from '@angular/core';
import { UtilsService } from '../../core/services/utils.service';
import { HttpMenuService } from './http-menu.service';
import { Logger } from '../../core/services/logger.service';
export class LocalizationService {
    /**
     * @param {?} httpMenuService
     * @param {?} utils
     * @param {?} logger
     */
    constructor(httpMenuService, utils, logger) {
        this.httpMenuService = httpMenuService;
        this.utils = utils;
        this.logger = logger;
        this.localizedElements = undefined;
        this.localizationsLoaded = new EventEmitter();
        this.logger.debug('LocalizationService instantiated - ' + Math.round(new Date().getTime() / 1000));
    }
    /**
     * @param {?} needLoginThread
     * @return {?}
     */
    loadLocalizedElements(needLoginThread) {
        if (this.localizedElements != undefined)
            return this.localizedElements;
        let /** @type {?} */ subs = this.httpMenuService.loadLocalizedElements(needLoginThread).subscribe(result => {
            this.localizedElements = result.LocalizedElements;
            this.localizationsLoaded.emit();
            subs.unsubscribe();
        });
    }
}
// //---------------------------------------------------------------------------------------------
// getLocalizedElement(key) {
//     if (this.localizedElements == undefined || this.localizedElements.LocalizedElement == undefined)
//         return undefined;
//     var allElements = this.localizedElements.LocalizedElement;
//     for (var i = 0; i < allElements.length; i++) {
//         if (allElements[i].key == key) {
//             return allElements[i].value;
//         }
//     };
//     return key;
// }
LocalizationService.decorators = [
    { type: Injectable },
];
/**
 * @nocollapse
 */
LocalizationService.ctorParameters = () => [
    { type: HttpMenuService, },
    { type: UtilsService, },
    { type: Logger, },
];
function LocalizationService_tsickle_Closure_declarations() {
    /** @type {?} */
    LocalizationService.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    LocalizationService.ctorParameters;
    /** @type {?} */
    LocalizationService.prototype.localizedElements;
    /** @type {?} */
    LocalizationService.prototype.localizationsLoaded;
    /** @type {?} */
    LocalizationService.prototype.httpMenuService;
    /** @type {?} */
    LocalizationService.prototype.utils;
    /** @type {?} */
    LocalizationService.prototype.logger;
}
;
