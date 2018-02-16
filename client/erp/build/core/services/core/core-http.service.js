/**
 * @fileoverview added by tsickle
 * @suppress {checkTypes} checked by tsc
 */
import { Injectable } from '@angular/core';
import { HttpService } from '@taskbuilder/core';
var CoreHttpService = (function () {
    function CoreHttpService(httpService) {
        this.httpService = httpService;
        this.controllerRoute = '/erp-core/';
    }
    /**
     * @param {?} vat
     * @return {?}
     */
    CoreHttpService.prototype.isVatDuplicate = /**
     * @param {?} vat
     * @return {?}
     */
    function (vat) {
        return this.httpService.execPost(this.controllerRoute, 'CheckVatDuplicate', vat);
    };
    /**
     * @param {?} countryCode
     * @param {?} vatNumber
     * @return {?}
     */
    CoreHttpService.prototype.checkVatEU = /**
     * @param {?} countryCode
     * @param {?} vatNumber
     * @return {?}
     */
    function (countryCode, vatNumber) {
        return this.httpService.execPost(this.controllerRoute, 'CheckVatEU', { 'countryCode': countryCode, 'vatNumber': vatNumber });
    };
    /**
     * @param {?} vatNumber
     * @param {?} date
     * @return {?}
     */
    CoreHttpService.prototype.checkVatRO = /**
     * @param {?} vatNumber
     * @param {?} date
     * @return {?}
     */
    function (vatNumber, date) {
        return this.httpService.execPost(this.controllerRoute, 'CheckVatRO', { 'cui': vatNumber, 'data': date });
    };
    CoreHttpService.decorators = [
        { type: Injectable },
    ];
    /** @nocollapse */
    CoreHttpService.ctorParameters = function () { return [
        { type: HttpService, },
    ]; };
    return CoreHttpService;
}());
export { CoreHttpService };
function CoreHttpService_tsickle_Closure_declarations() {
    /** @type {!Array<{type: !Function, args: (undefined|!Array<?>)}>} */
    CoreHttpService.decorators;
    /**
     * @nocollapse
     * @type {function(): !Array<(null|{type: ?, decorators: (undefined|!Array<{type: !Function, args: (undefined|!Array<?>)}>)})>}
     */
    CoreHttpService.ctorParameters;
    /** @type {?} */
    CoreHttpService.prototype.controllerRoute;
    /** @type {?} */
    CoreHttpService.prototype.httpService;
}
