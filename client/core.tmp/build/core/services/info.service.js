import { Injectable } from '@angular/core';
import { HttpService } from './http.service';
export class InfoService {
    /**
     * @param {?} httpService
     */
    constructor(httpService) { }
}
InfoService.decorators = [
    { type: Injectable },
];
/**
 * @nocollapse
 */
InfoService.ctorParameters = () => [
    { type: HttpService, },
];
function InfoService_tsickle_Closure_declarations() {
    /** @type {?} */
    InfoService.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    InfoService.ctorParameters;
    /** @type {?} */
    InfoService.prototype.desktop;
}
