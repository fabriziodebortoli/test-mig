import { Component, Input } from '@angular/core';
import { ControlComponent } from './../control.component';
export class RadioComponent extends ControlComponent {
}
RadioComponent.decorators = [
    { type: Component, args: [{
                selector: 'tb-radio',
                template: "<div class=\"tb-control tb-radio\"> <tb-caption caption=\"{{caption}}\" [for]=\"cmpId\"></tb-caption> <input class=\"k-radio\" id=\"{{cmpId}}\" type=\"radio\" [ngModel]=\"model?.value\" [disabled]=\"!model?.enabled\" (ngModelChange)=\"model.value=$event\"> <label class=\"k-radio-label\" for=\"{{cmpId}}\">&nbsp;</label> </div>",
                styles: [""]
            },] },
];
/**
 * @nocollapse
 */
RadioComponent.ctorParameters = () => [];
RadioComponent.propDecorators = {
    'name': [{ type: Input },],
};
function RadioComponent_tsickle_Closure_declarations() {
    /** @type {?} */
    RadioComponent.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    RadioComponent.ctorParameters;
    /** @type {?} */
    RadioComponent.propDecorators;
    /** @type {?} */
    RadioComponent.prototype.name;
}
