import { Component, Input } from '@angular/core';
export class PlaceholderComponent {
    constructor() { }
}
PlaceholderComponent.decorators = [
    { type: Component, args: [{
                selector: 'tb-placeholder',
                template: "<label for=\"{{forCmpID}}\">  {{placeHolder}} </label>",
                styles: [""]
            },] },
];
/**
 * @nocollapse
 */
PlaceholderComponent.ctorParameters = () => [];
PlaceholderComponent.propDecorators = {
    'placeHolder': [{ type: Input },],
};
function PlaceholderComponent_tsickle_Closure_declarations() {
    /** @type {?} */
    PlaceholderComponent.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    PlaceholderComponent.ctorParameters;
    /** @type {?} */
    PlaceholderComponent.propDecorators;
    /** @type {?} */
    PlaceholderComponent.prototype.placeHolder;
}
