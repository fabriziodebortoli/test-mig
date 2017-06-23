import { Component, Input } from '@angular/core';
import { ControlComponent } from './../control.component';
export class MaskedTextBoxComponent extends ControlComponent {
}
MaskedTextBoxComponent.decorators = [
    { type: Component, args: [{
                selector: 'tb-masked-text-box',
                template: "<div class=\"tb-control tb-masked-text-box\"> <tb-caption caption=\"{{caption}}\" [for]=\"cmpId\"></tb-caption> <kendo-maskedtextbox [ngModel]=\"model?.value\" (ngModelChange)=\"model.value=$event\" [disabled]=\"!model?.enabled\" [mask]=\"mask\" [style.width.px]=\"width\"></kendo-maskedtextbox> </div>",
                styles: [""]
            },] },
];
/**
 * @nocollapse
 */
MaskedTextBoxComponent.ctorParameters = () => [];
MaskedTextBoxComponent.propDecorators = {
    'forCmpID': [{ type: Input },],
    'disabled': [{ type: Input },],
    'mask': [{ type: Input },],
    'width': [{ type: Input },],
};
function MaskedTextBoxComponent_tsickle_Closure_declarations() {
    /** @type {?} */
    MaskedTextBoxComponent.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    MaskedTextBoxComponent.ctorParameters;
    /** @type {?} */
    MaskedTextBoxComponent.propDecorators;
    /** @type {?} */
    MaskedTextBoxComponent.prototype.forCmpID;
    /** @type {?} */
    MaskedTextBoxComponent.prototype.disabled;
    /** @type {?} */
    MaskedTextBoxComponent.prototype.mask;
    /** @type {?} */
    MaskedTextBoxComponent.prototype.width;
}
