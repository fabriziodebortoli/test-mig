import { Component, Input } from '@angular/core';
export class M4IconComponent {
    constructor() {
        this.icon = '';
    }
}
M4IconComponent.decorators = [
    { type: Component, args: [{
                selector: 'm4-icon',
                styles: [""],
                template: `<i class="m4-icon m4-{{icon}}"></i>`
            },] },
];
/**
 * @nocollapse
 */
M4IconComponent.ctorParameters = () => [];
M4IconComponent.propDecorators = {
    'icon': [{ type: Input, args: ['icon',] },],
};
function M4IconComponent_tsickle_Closure_declarations() {
    /** @type {?} */
    M4IconComponent.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    M4IconComponent.ctorParameters;
    /** @type {?} */
    M4IconComponent.propDecorators;
    /** @type {?} */
    M4IconComponent.prototype.icon;
}
