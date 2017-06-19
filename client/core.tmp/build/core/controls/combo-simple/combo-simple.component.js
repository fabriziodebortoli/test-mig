import { Component, Input, Output, EventEmitter } from '@angular/core';
import { ControlComponent } from './../control.component';
export class ComboSimpleComponent extends ControlComponent {
    constructor() {
        super(...arguments);
        this.items = [];
        this.changed = new EventEmitter();
    }
    /**
     * @param {?} value
     * @return {?}
     */
    selectionChange(value) {
        this.model.value = value.code;
        this.changed.emit(this);
    }
}
ComboSimpleComponent.decorators = [
    { type: Component, args: [{
                selector: 'tb-combo-simple',
                template: "<div class=\"tb-control tb-combo-simple\"> <tb-caption caption=\"{{caption}}\" [for]=\"cmpId\"></tb-caption> <kendo-dropdownlist [data]=\"items\" [textField]=\"'description'\" [disabled]=\"!model?.enabled\" [valueField]=\"'code'\" [defaultItem]=\"defaultItem\" (selectionChange)=\"selectionChange($event)\"> <template kendoDropDownListNoDataTemplate> <h4><span class=\"k-icon k-i-warning\"></span><br /><br /> No data here</h4> </template> </kendo-dropdownlist> </div>",
                styles: [".k-i-warning { font-size: 2.5em; } h4 { font-size: 1em; } "]
            },] },
];
/**
 * @nocollapse
 */
ComboSimpleComponent.ctorParameters = () => [];
ComboSimpleComponent.propDecorators = {
    'items': [{ type: Input },],
    'defaultItem': [{ type: Input },],
    'changed': [{ type: Output, args: ['changed',] },],
};
function ComboSimpleComponent_tsickle_Closure_declarations() {
    /** @type {?} */
    ComboSimpleComponent.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    ComboSimpleComponent.ctorParameters;
    /** @type {?} */
    ComboSimpleComponent.propDecorators;
    /** @type {?} */
    ComboSimpleComponent.prototype.items;
    /** @type {?} */
    ComboSimpleComponent.prototype.defaultItem;
    /** @type {?} */
    ComboSimpleComponent.prototype.changed;
}
