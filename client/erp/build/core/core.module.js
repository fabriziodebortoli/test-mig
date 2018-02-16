/**
 * @fileoverview added by tsickle
 * @suppress {checkTypes} checked by tsc
 */
import { NgModule, Optional, SkipSelf } from '@angular/core';
import { CoreHttpService } from './services/core/core-http.service';
import { ItemsHttpService } from './services/items/items-http.service';
import { WmsHttpService } from './services/wms/wms-http.service';
export { CoreHttpService } from './services/core/core-http.service';
export { ItemsHttpService } from './services/items/items-http.service';
export { WmsHttpService } from './services/wms/wms-http.service';
export { ClipboardEventHelper, KeyboardEventHelper } from './u/helpers';
export var /** @type {?} */ ERP_SERVICES = [
    CoreHttpService,
    ItemsHttpService,
    WmsHttpService
];
var ERPCoreModule = (function () {
    function ERPCoreModule(parentModule) {
        if (parentModule) {
            throw new Error('ERPCoreModule is already loaded. Import it in the AppModule only');
        }
    }
    /**
     * @return {?}
     */
    ERPCoreModule.forRoot = /**
     * @return {?}
     */
    function () {
        return {
            ngModule: ERPCoreModule,
            providers: [ERP_SERVICES]
        };
    };
    ERPCoreModule.decorators = [
        { type: NgModule, args: [{
                    providers: [ERP_SERVICES]
                },] },
    ];
    /** @nocollapse */
    ERPCoreModule.ctorParameters = function () { return [
        { type: ERPCoreModule, decorators: [{ type: Optional }, { type: SkipSelf },] },
    ]; };
    return ERPCoreModule;
}());
export { ERPCoreModule };
function ERPCoreModule_tsickle_Closure_declarations() {
    /** @type {!Array<{type: !Function, args: (undefined|!Array<?>)}>} */
    ERPCoreModule.decorators;
    /**
     * @nocollapse
     * @type {function(): !Array<(null|{type: ?, decorators: (undefined|!Array<{type: !Function, args: (undefined|!Array<?>)}>)})>}
     */
    ERPCoreModule.ctorParameters;
}
