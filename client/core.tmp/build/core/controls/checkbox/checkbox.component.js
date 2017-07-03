import { Component } from '@angular/core';
import { ControlComponent } from './../control.component';
export class CheckBoxComponent extends ControlComponent {
}
CheckBoxComponent.decorators = [
    { type: Component, args: [{
                selector: 'tb-checkbox',
                template: "<div class=\"tb-control tb-checkbox\"> <tb-caption caption=\"{{caption}}\" [for]=\"cmpId\"></tb-caption> <input type=\"checkbox\" id=\"{{cmpId}}\" [ngModel]=\"model?.value\" [disabled]=\"!model?.enabled\" (ngModelChange)=\"model.value=$event\" class=\"k-checkbox\"> <label class=\"k-checkbox-label\" for=\"{{cmpId}}\">&nbsp;</label> </div>",
                styles: [".mat-checkbox-input { border: 1px solid red; } "]
            },] },
];
/**
 * @nocollapse
 */
CheckBoxComponent.ctorParameters = () => [];
function CheckBoxComponent_tsickle_Closure_declarations() {
    /** @type {?} */
    CheckBoxComponent.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    CheckBoxComponent.ctorParameters;
}
