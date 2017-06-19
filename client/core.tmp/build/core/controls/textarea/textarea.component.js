import { Component, Input } from '@angular/core';
import { ControlComponent } from './../control.component';
export class TextareaComponent extends ControlComponent {
    constructor() {
        super();
        this.readonly = false;
    }
    /**
     * @return {?}
     */
    getCorrectHeight() {
        return isNaN(this.height) ? this.height.toString() : this.height + 'px';
        ;
    }
    /**
     * @return {?}
     */
    getCorrectWidth() {
        return isNaN(this.width) ? this.width.toString() : this.width + 'px';
    }
}
TextareaComponent.decorators = [
    { type: Component, args: [{
                selector: 'tb-textarea',
                template: "<div class=\"tb-control tb-textarea\"> <tb-caption caption=\"{{caption}}\" [for]=\"cmpId\"></tb-caption> <textarea name=\"{{cmpId}}\" [readonly]=\"readonly\" [ngModel]=\"model?.value\"  (ngModelChange)=\"model.value=$event\" [disabled]=\"!model?.enabled\" [ngStyle]=\"{'height': getCorrectHeight(), 'width':  getCorrectWidth()}\"></textarea> <ng-container #contextMenu></ng-container> </div>",
                styles: ["textarea { width: 100%; background: #ffffde; border: 1px solid #ddd; } textarea[disabled] { width: 100%; background-color: #f3f3f3; border: 1px solid #ddd; font-weight: bold; color: #9c9c9c; font-family: inherit; } "]
            },] },
];
/**
 * @nocollapse
 */
TextareaComponent.ctorParameters = () => [];
TextareaComponent.propDecorators = {
    'readonly': [{ type: Input, args: ['readonly',] },],
    'width': [{ type: Input },],
    'height': [{ type: Input },],
};
function TextareaComponent_tsickle_Closure_declarations() {
    /** @type {?} */
    TextareaComponent.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    TextareaComponent.ctorParameters;
    /** @type {?} */
    TextareaComponent.propDecorators;
    /** @type {?} */
    TextareaComponent.prototype.readonly;
    /** @type {?} */
    TextareaComponent.prototype.width;
    /** @type {?} */
    TextareaComponent.prototype.height;
}
