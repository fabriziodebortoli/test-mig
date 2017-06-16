import { Component } from '@angular/core';
import { ControlComponent } from '../control.component';
export class ColorPickerComponent extends ControlComponent {
    constructor() {
        super();
    }
}
ColorPickerComponent.decorators = [
    { type: Component, args: [{
                selector: 'tb-color-picker',
                template: "<div class=\"tb-control tb-color-picker\"> <tb-caption caption=\"{{caption}}\" [for]=\"cmpId\"></tb-caption> <input id=\"{{cmpId}}\" type=\"text\" (blur)=\"onBlur()\" [disabled]=\"!model?.enabled\" [ngModel]=\"model?.value\" (ngModelChange)=\"model.value=$event\" [placeholder]=\"caption\" class=\"tb-color-picker\" /> </div>",
                styles: [""]
            },] },
];
/**
 * @nocollapse
 */
ColorPickerComponent.ctorParameters = () => [];
function ColorPickerComponent_tsickle_Closure_declarations() {
    /** @type {?} */
    ColorPickerComponent.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    ColorPickerComponent.ctorParameters;
}
