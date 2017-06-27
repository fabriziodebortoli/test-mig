import { Component } from '@angular/core';
import { ControlComponent } from '../control.component';
export class UnknownComponent extends ControlComponent {
    constructor() {
        super();
    }
}
UnknownComponent.decorators = [
    { type: Component, args: [{
                selector: 'tb-unknown',
                template: "<p> unknown works! </p> ",
                styles: [""]
            },] },
];
/**
 * @nocollapse
 */
UnknownComponent.ctorParameters = () => [];
function UnknownComponent_tsickle_Closure_declarations() {
    /** @type {?} */
    UnknownComponent.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    UnknownComponent.ctorParameters;
}
