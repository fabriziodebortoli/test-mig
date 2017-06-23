import { Component, Input } from '@angular/core';
import { TextComponent } from './../text/text.component';
export class LabelStaticComponent extends TextComponent {
    constructor() {
        super(...arguments);
        this.caption = '';
    }
    /**
     * @return {?}
     */
    ngOnInit() { }
}
LabelStaticComponent.decorators = [
    { type: Component, args: [{
                selector: 'tb-label-static',
                template: "<div> <tb-text [model]=\"model\" [width]=\"width\"></tb-text> </div>",
                styles: [""]
            },] },
];
/**
 * @nocollapse
 */
LabelStaticComponent.ctorParameters = () => [];
LabelStaticComponent.propDecorators = {
    'caption': [{ type: Input },],
    'width': [{ type: Input },],
};
function LabelStaticComponent_tsickle_Closure_declarations() {
    /** @type {?} */
    LabelStaticComponent.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    LabelStaticComponent.ctorParameters;
    /** @type {?} */
    LabelStaticComponent.propDecorators;
    /** @type {?} */
    LabelStaticComponent.prototype.caption;
    /** @type {?} */
    LabelStaticComponent.prototype.width;
}
