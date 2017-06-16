import { Component, Input } from '@angular/core';
import { ControlComponent } from './../control.component';
export class PhoneComponent extends ControlComponent {
}
PhoneComponent.decorators = [
    { type: Component, args: [{
                selector: 'tb-phone',
                template: "<div class=\"tb-control tb-phone\"> <tb-caption caption=\"{{caption}}\" [for]=\"cmpId\"></tb-caption> <kendo-maskedtextbox [mask]=\"mask\" [ngModel]=\"model?.value\" (ngModelChange)=\"model.value=$event\" [disabled]=\"!model?.enabled\"></kendo-maskedtextbox> </div>",
                styles: [""]
            },] },
];
/**
 * @nocollapse
 */
PhoneComponent.ctorParameters = () => [];
PhoneComponent.propDecorators = {
    'mask': [{ type: Input },],
};
function PhoneComponent_tsickle_Closure_declarations() {
    /** @type {?} */
    PhoneComponent.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    PhoneComponent.ctorParameters;
    /** @type {?} */
    PhoneComponent.propDecorators;
    /** @type {?} */
    PhoneComponent.prototype.mask;
}
