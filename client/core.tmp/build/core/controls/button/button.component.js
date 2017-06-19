import { Component } from '@angular/core';
import { ControlComponent } from './../control.component';
export class ButtonComponent extends ControlComponent {
}
ButtonComponent.decorators = [
    { type: Component, args: [{
                selector: 'tb-button',
                template: "<button kendoButton id=\"{{cmpId}}\">{{ caption }}</button>",
                styles: [""]
            },] },
];
/**
 * @nocollapse
 */
ButtonComponent.ctorParameters = () => [];
function ButtonComponent_tsickle_Closure_declarations() {
    /** @type {?} */
    ButtonComponent.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    ButtonComponent.ctorParameters;
}
