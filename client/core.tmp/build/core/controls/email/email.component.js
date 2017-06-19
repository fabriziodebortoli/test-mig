import { Component, Input } from '@angular/core';
import { ControlComponent } from './../control.component';
import { EventDataService } from './../../services/eventdata.service';
export class EmailComponent extends ControlComponent {
    /**
     * @param {?} eventData
     */
    constructor(eventData) {
        super();
        this.eventData = eventData;
        this.readonly = false;
        this.showError = '';
    }
    /**
     * @return {?}
     */
    ngOnInit() {
    }
    /**
     * @param {?} val
     * @return {?}
     */
    onChange(val) {
        this.onUpdateNgModel(val);
    }
    /**
     * @param {?} newValue
     * @return {?}
     */
    onUpdateNgModel(newValue) {
        if (!this.modelValid()) {
            this.model = { enable: 'true', value: '' };
        }
        this.model.value = newValue;
    }
    /**
     * @return {?}
     */
    ngAfterViewInit() {
        if (this.modelValid()) {
            this.onUpdateNgModel(this.model.value);
        }
    }
    /**
     * @return {?}
     */
    ngOnChanges() {
        if (this.modelValid()) {
            this.onUpdateNgModel(this.model.value);
        }
    }
    /**
     * @return {?}
     */
    modelValid() {
        return this.model !== undefined && this.model !== null;
    }
    /**
     * @return {?}
     */
    onBlur() {
        this.constraint = new RegExp('^[a-zA-Z0-9_\+-]+(\.[a-zA-Z0-9_\+-]+)*@[a-zA-Z0-9-]+(\.[a-zA-Z0-9-]+)*\.([a-zA-Z]{2,})$', 'i');
        if (!this.constraint.test(this.model.value)) {
            this.errorMessage = 'Input not in correct form';
            this.showError = 'inputError';
        }
        else {
            this.errorMessage = '';
            this.showError = '';
        }
        this.eventData.change.emit(this.cmpId);
    }
}
EmailComponent.decorators = [
    { type: Component, args: [{
                selector: 'tb-email',
                template: "<div class=\"tb-control tb-email\"> <tb-caption caption=\"{{caption}}\" [for]=\"cmpId\"></tb-caption> <!--<input id=\"{{cmpId}}\" type=\"text\" [ngClass]=\"showError\" class=\"tb-text\" (blur)=\"onBlur()\" [disabled]=\"!model?.enabled\" [ngModel]=\"model?.value\" (ngModelChange)=\"model.value=$event\" [readonly]=\"readonly\" />--> <kendo-maskedtextbox id=\"{{cmpId}}\"  [mask]=\"mask\" [ngClass]=\"showError\" class=\"tb-text\" [ngModel]=\"model?.value\" (blur)=\"onBlur()\" [disabled]=\"!(model?.enabled)\" (ngModelChange)=\"model.value=$event\"  ></kendo-maskedtextbox> <div class=\"has-error\">{{errorMessage}}</div> </div>",
                styles: [""]
            },] },
];
/**
 * @nocollapse
 */
EmailComponent.ctorParameters = () => [
    { type: EventDataService, },
];
EmailComponent.propDecorators = {
    'readonly': [{ type: Input, args: ['readonly',] },],
};
function EmailComponent_tsickle_Closure_declarations() {
    /** @type {?} */
    EmailComponent.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    EmailComponent.ctorParameters;
    /** @type {?} */
    EmailComponent.propDecorators;
    /** @type {?} */
    EmailComponent.prototype.readonly;
    /** @type {?} */
    EmailComponent.prototype.errorMessage;
    /** @type {?} */
    EmailComponent.prototype.showError;
    /** @type {?} */
    EmailComponent.prototype.constraint;
    /** @type {?} */
    EmailComponent.prototype.eventData;
}
