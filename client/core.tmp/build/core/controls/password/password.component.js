import { Component, Input } from '@angular/core';
import { ControlComponent } from './../control.component';
export class PasswordComponent extends ControlComponent {
}
PasswordComponent.decorators = [
    { type: Component, args: [{
                selector: 'tb-password',
                template: "<div class=\"tb-control tb-password\"> <tb-caption caption=\"{{caption}}\" [for]=\"cmpId\"></tb-caption> <input type=\"password\" [ngModel]=\"model?.value\" [disabled]=\"!model?.enabled\" (ngModelChange)=\"model.value=$event\" /> </div>",
                styles: [""]
            },] },
];
/**
 * @nocollapse
 */
PasswordComponent.ctorParameters = () => [];
PasswordComponent.propDecorators = {
    'forCmpID': [{ type: Input },],
};
function PasswordComponent_tsickle_Closure_declarations() {
    /** @type {?} */
    PasswordComponent.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    PasswordComponent.ctorParameters;
    /** @type {?} */
    PasswordComponent.propDecorators;
    /** @type {?} */
    PasswordComponent.prototype.forCmpID;
}
