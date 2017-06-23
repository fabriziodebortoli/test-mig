import { Component, Input } from '@angular/core';
import { ControlComponent } from './../control.component';
import { EventDataService } from './../../services/eventdata.service';
export class LinkComponent extends ControlComponent {
    /**
     * @param {?} eventData
     */
    constructor(eventData) {
        super();
        this.eventData = eventData;
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
        this.selectedValue = newValue;
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
        this.constraint = new RegExp('((http|https)(:\/\/))?([a-zA-Z0-9]+[.]{1}){2}[a-zA-z0-9]+(\/{1}[a-zA-Z0-9]+)*\/?', 'i');
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
LinkComponent.decorators = [
    { type: Component, args: [{
                selector: 'tb-link',
                template: "<div class=\"tb-control tb-link\"> <tb-caption caption=\"{{caption}}\" [for]=\"cmpId\"></tb-caption> <kendo-maskedtextbox [ngClass]=\"showError\" [(ngModel)]=\"selectedValue\" required=\"required\" (blur)=\"onBlur()\" [disabled]=\"!model?.enabled\" (ngModelChange)=\"model.value=$event\"></kendo-maskedtextbox> <ng-container #stateButton></ng-container> <div class=\"has-error\">{{errorMessage}}</div> </div>",
                styles: [""]
            },] },
];
/**
 * @nocollapse
 */
LinkComponent.ctorParameters = () => [
    { type: EventDataService, },
];
LinkComponent.propDecorators = {
    'pattern': [{ type: Input },],
};
function LinkComponent_tsickle_Closure_declarations() {
    /** @type {?} */
    LinkComponent.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    LinkComponent.ctorParameters;
    /** @type {?} */
    LinkComponent.propDecorators;
    /** @type {?} */
    LinkComponent.prototype.selectedValue;
    /** @type {?} */
    LinkComponent.prototype.pattern;
    /** @type {?} */
    LinkComponent.prototype.constraint;
    /** @type {?} */
    LinkComponent.prototype.errorMessage;
    /** @type {?} */
    LinkComponent.prototype.showError;
    /** @type {?} */
    LinkComponent.prototype.eventData;
}
