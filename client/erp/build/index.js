/**
 * @fileoverview added by tsickle
 * @suppress {checkTypes} checked by tsc
 */
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ERP_SERVICES } from './core/core.module';
import { TbSharedModule } from '@taskbuilder/core';
export { CoreHttpService, ItemsHttpService, WmsHttpService, ClipboardEventHelper, KeyboardEventHelper, ERP_SERVICES, ERPCoreModule } from './core/core.module';
import { ERPSharedModule } from './shared/shared.module';
export { NoSpacesEditComponent, TaxIdEditComponent, FiscalCodeEditComponent, NumberEditWithFillerComponent, EsrComponent, StrBinEditComponent, NumbererComponent, ItemEditComponent, AutoSearchEditComponent, ChartOfAccountComponent, KeyValueFilterPipe, ERPSharedModule } from './shared/shared.module';
var /** @type {?} */ ERP_MODULES = [ERPSharedModule];
var ERPModule = (function () {
    function ERPModule() {
    }
    /**
     * @return {?}
     */
    ERPModule.forRoot = /**
     * @return {?}
     */
    function () {
        return {
            ngModule: ERPModule,
            providers: [ERP_SERVICES]
        };
    };
    ERPModule.decorators = [
        { type: NgModule, args: [{
                    imports: [
                        CommonModule,
                        TbSharedModule,
                        ERP_MODULES
                    ],
                    declarations: [],
                    exports: [
                        ERP_MODULES
                    ],
                    providers: [ERP_SERVICES]
                },] },
    ];
    /** @nocollapse */
    ERPModule.ctorParameters = function () { return []; };
    return ERPModule;
}());
export { ERPModule };
function ERPModule_tsickle_Closure_declarations() {
    /** @type {!Array<{type: !Function, args: (undefined|!Array<?>)}>} */
    ERPModule.decorators;
    /**
     * @nocollapse
     * @type {function(): !Array<(null|{type: ?, decorators: (undefined|!Array<{type: !Function, args: (undefined|!Array<?>)}>)})>}
     */
    ERPModule.ctorParameters;
}
