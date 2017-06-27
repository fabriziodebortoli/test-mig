import { Component, Input } from '@angular/core';
export class StateButtonComponent {
    constructor() { }
    /**
     * @return {?}
     */
    ngOnInit() {
    }
    /**
     * @return {?}
     */
    onClick() {
    }
}
StateButtonComponent.decorators = [
    { type: Component, args: [{
                selector: 'tb-state-button',
                template: "<md-icon (click)=\"onClick()\" id=\"{{button.IDD_Comand}}\">{{button.iconFont}}</md-icon>",
                styles: [""]
            },] },
];
/**
 * @nocollapse
 */
StateButtonComponent.ctorParameters = () => [];
StateButtonComponent.propDecorators = {
    'button': [{ type: Input },],
};
function StateButtonComponent_tsickle_Closure_declarations() {
    /** @type {?} */
    StateButtonComponent.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    StateButtonComponent.ctorParameters;
    /** @type {?} */
    StateButtonComponent.propDecorators;
    /** @type {?} */
    StateButtonComponent.prototype.button;
}
