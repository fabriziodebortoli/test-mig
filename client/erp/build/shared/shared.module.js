/**
 * @fileoverview added by tsickle
 * @suppress {checkTypes} checked by tsc
 */
import { FormsModule } from '@angular/forms';
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TaskbuilderCoreModule } from '@taskbuilder/core';
import { NoSpacesEditComponent } from './controls/no-spaces/no-spaces.component';
import { TaxIdEditComponent } from './controls/taxid-edit/taxid-edit.component';
import { FiscalCodeEditComponent } from './controls/fiscalcode-edit/fiscalcode-edit.component';
import { NumberEditWithFillerComponent } from './controls/number-edit-with-filler/tb-number-edit-with-filler.component';
import { EsrComponent } from './controls/esr/esr.component';
import { StrBinEditComponent } from './controls/str-bin-edit/str-bin-edit.component';
import { NumbererComponent } from './controls/numberer/numberer.component';
import { ItemEditComponent } from './controls/item-edit/item-edit.component';
import { AutoSearchEditComponent } from './controls/auto-search-edit/auto-search-edit.component';
import { ChartOfAccountComponent } from './controls/chart-of-account/chart-of-account.component';
import { KeyValueFilterPipe } from './pipes/key-value-filter.pipe';
export { NoSpacesEditComponent } from './controls/no-spaces/no-spaces.component';
export { TaxIdEditComponent } from './controls/taxid-edit/taxid-edit.component';
export { FiscalCodeEditComponent } from './controls/fiscalcode-edit/fiscalcode-edit.component';
export { NumberEditWithFillerComponent } from './controls/number-edit-with-filler/tb-number-edit-with-filler.component';
export { EsrComponent } from './controls/esr/esr.component';
export { StrBinEditComponent } from './controls/str-bin-edit/str-bin-edit.component';
export { NumbererComponent } from './controls/numberer/numberer.component';
export { ItemEditComponent } from './controls/item-edit/item-edit.component';
export { AutoSearchEditComponent } from './controls/auto-search-edit/auto-search-edit.component';
export { ChartOfAccountComponent } from './controls/chart-of-account/chart-of-account.component';
export { KeyValueFilterPipe } from './pipes/key-value-filter.pipe';
var /** @type {?} */ ERP_COMPONENTS = [
    AutoSearchEditComponent,
    EsrComponent,
    NumbererComponent,
    ItemEditComponent,
    NoSpacesEditComponent,
    NumberEditWithFillerComponent,
    StrBinEditComponent,
    TaxIdEditComponent,
    FiscalCodeEditComponent,
    ChartOfAccountComponent
];
var /** @type {?} */ ERP_PIPES = [
    KeyValueFilterPipe
];
var ERPSharedModule = (function () {
    function ERPSharedModule() {
    }
    ERPSharedModule.decorators = [
        { type: NgModule, args: [{
                    imports: [FormsModule, TaskbuilderCoreModule, CommonModule],
                    declarations: [ERP_COMPONENTS, ERP_PIPES],
                    exports: [ERP_COMPONENTS, ERP_PIPES],
                    providers: []
                },] },
    ];
    /** @nocollapse */
    ERPSharedModule.ctorParameters = function () { return []; };
    return ERPSharedModule;
}());
export { ERPSharedModule };
function ERPSharedModule_tsickle_Closure_declarations() {
    /** @type {!Array<{type: !Function, args: (undefined|!Array<?>)}>} */
    ERPSharedModule.decorators;
    /**
     * @nocollapse
     * @type {function(): !Array<(null|{type: ?, decorators: (undefined|!Array<{type: !Function, args: (undefined|!Array<?>)}>)})>}
     */
    ERPSharedModule.ctorParameters;
}