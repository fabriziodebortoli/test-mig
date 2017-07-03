import { Component, Input, Output, EventEmitter } from '@angular/core';
import { TbComponent } from '../components/index';
export class ControlComponent extends TbComponent {
    constructor() {
        super(...arguments);
        this.validators = [];
        this.blur = new EventEmitter();
    }
    /**
     * @return {?}
     */
    get model() {
        return this._model;
    }
    /**
     * @param {?} model
     * @return {?}
     */
    set model(model) {
        if (model == undefined)
            return;
        this._model = model;
        this.value = model.value;
    }
}
ControlComponent.decorators = [
    { type: Component, args: [{
                template: ''
            },] },
];
/**
 * @nocollapse
 */
ControlComponent.ctorParameters = () => [];
ControlComponent.propDecorators = {
    'caption': [{ type: Input },],
    'args': [{ type: Input },],
    'validators': [{ type: Input },],
    'value': [{ type: Input },],
    'blur': [{ type: Output, args: ['blur',] },],
    'model': [{ type: Input },],
};
function ControlComponent_tsickle_Closure_declarations() {
    /** @type {?} */
    ControlComponent.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    ControlComponent.ctorParameters;
    /** @type {?} */
    ControlComponent.propDecorators;
    /** @type {?} */
    ControlComponent.prototype._model;
    /** @type {?} */
    ControlComponent.prototype.caption;
    /** @type {?} */
    ControlComponent.prototype.args;
    /** @type {?} */
    ControlComponent.prototype.validators;
    /** @type {?} */
    ControlComponent.prototype.value;
    /** @type {?} */
    ControlComponent.prototype.blur;
}
