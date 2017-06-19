import { Component, Input } from '@angular/core';
import { ControlComponent } from './../control.component';
export class CaptionComponent extends ControlComponent {
}
CaptionComponent.decorators = [
    { type: Component, args: [{
                selector: 'tb-caption',
                template: "<label for=\"{{for}}\" class=\"control-label\">{{caption}}</label>",
                styles: [""]
            },] },
];
/**
 * @nocollapse
 */
CaptionComponent.ctorParameters = () => [];
CaptionComponent.propDecorators = {
    'for': [{ type: Input },],
};
function CaptionComponent_tsickle_Closure_declarations() {
    /** @type {?} */
    CaptionComponent.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    CaptionComponent.ctorParameters;
    /** @type {?} */
    CaptionComponent.propDecorators;
    /** @type {?} */
    CaptionComponent.prototype.for;
}
