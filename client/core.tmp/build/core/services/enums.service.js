import { Injectable } from '@angular/core';
import { HttpService } from './http.service';
export class EnumsService {
    /**
     * @param {?} httpService
     */
    constructor(httpService) {
        this.httpService = httpService;
    }
    /**
     * @return {?}
     */
    getEnumsTable() {
        let /** @type {?} */ subs = this.getEnumsTableSubscription = this.httpService.getEnumsTable().subscribe((json) => {
            this.enumsTable = json.enums;
            subs.unsubscribe();
        });
    }
    /**
     * @param {?} storedValue
     * @return {?}
     */
    getEnumsItem(storedValue) {
        if (this.enumsTable == undefined)
            return;
        for (let /** @type {?} */ index = 0; index < this.enumsTable.tags.length; index++) {
            let /** @type {?} */ tag = this.enumsTable.tags[index];
            if (tag != undefined) {
                for (let /** @type {?} */ j = 0; j < tag.items.length; j++) {
                    if (tag.items[j].stored == storedValue)
                        return tag.items[j];
                }
            }
        }
        return undefined;
    }
    /**
     * @param {?} tag
     * @return {?}
     */
    getItemsFromTag(tag) {
        if (this.enumsTable == undefined)
            return;
        for (let /** @type {?} */ index = 0; index < this.enumsTable.tags.length; index++) {
            let /** @type {?} */ currentTag = this.enumsTable.tags[index];
            if (currentTag != undefined && currentTag.value == tag) {
                return currentTag.items;
            }
        }
        return undefined;
    }
    /**
     * @return {?}
     */
    dispose() {
        this.getEnumsTableSubscription.unsubscribe();
    }
}
EnumsService.decorators = [
    { type: Injectable },
];
/**
 * @nocollapse
 */
EnumsService.ctorParameters = () => [
    { type: HttpService, },
];
function EnumsService_tsickle_Closure_declarations() {
    /** @type {?} */
    EnumsService.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    EnumsService.ctorParameters;
    /** @type {?} */
    EnumsService.prototype.enumsTable;
    /** @type {?} */
    EnumsService.prototype.getEnumsTableSubscription;
    /** @type {?} */
    EnumsService.prototype.httpService;
}
