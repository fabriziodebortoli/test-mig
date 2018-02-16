/**
 * @fileoverview added by tsickle
 * @suppress {checkTypes} checked by tsc
 */
import { Injectable } from '@angular/core';
import { HttpService } from '@taskbuilder/core';
var WmsHttpService = (function () {
    function WmsHttpService(httpService) {
        this.httpService = httpService;
        this.controllerRoute = '/erp-core/';
    }
    /**
     * @param {?} zone
     * @param {?} storage
     * @return {?}
     */
    WmsHttpService.prototype.checkBinUsesStructure = /**
     * @param {?} zone
     * @param {?} storage
     * @return {?}
     */
    function (zone, storage) {
        return this.httpService.execPost(this.controllerRoute, 'CheckBinUsesStructure', { 'zone': zone, 'storage': storage });
    };
    WmsHttpService.decorators = [
        { type: Injectable },
    ];
    /** @nocollapse */
    WmsHttpService.ctorParameters = function () { return [
        { type: HttpService, },
    ]; };
    return WmsHttpService;
}());
export { WmsHttpService };
function WmsHttpService_tsickle_Closure_declarations() {
    /** @type {!Array<{type: !Function, args: (undefined|!Array<?>)}>} */
    WmsHttpService.decorators;
    /**
     * @nocollapse
     * @type {function(): !Array<(null|{type: ?, decorators: (undefined|!Array<{type: !Function, args: (undefined|!Array<?>)}>)})>}
     */
    WmsHttpService.ctorParameters;
    /** @type {?} */
    WmsHttpService.prototype.controllerRoute;
    /** @type {?} */
    WmsHttpService.prototype.httpService;
}
