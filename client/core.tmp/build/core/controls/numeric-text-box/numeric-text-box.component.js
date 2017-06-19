import { Component, Input } from '@angular/core';
import { ControlComponent } from './../control.component';
import { EventDataService } from './../../services/eventdata.service';
export class NumericTextBoxComponent extends ControlComponent {
    /**
     * @param {?} eventData
     */
    constructor(eventData) {
        super();
        this.eventData = eventData;
        this.constraint = new RegExp('\\d');
        this.showError = '';
        this.formatOptionsCurrency = {
            style: 'currency',
            currency: 'EUR',
            currencyDisplay: 'name'
        };
        this.formatOptionsInteger = {
            style: 'decimal'
        };
        this.formatOptionsDouble = 'F';
        this.formatOptionsPercent = {
            style: 'percent'
        };
    }
    /**
     * @return {?}
     */
    getDecimalsOptions() {
        switch (this.formatter) {
            case 'Integer':
            case 'Long':
                this.decimals = 0;
                break;
            case 'Money':
                this.decimals = 2;
                break;
            default: break;
        }
        return this.decimals;
    }
    /**
     * @return {?}
     */
    getFormatOptions() {
        switch (this.formatter) {
            case 'Integer':
            case 'Long':
                return this.formatOptionsInteger;
            case 'Double':
                return this.formatOptionsDouble;
            case 'Money':
                return this.formatOptionsCurrency;
            case 'Percent':
                return this.formatOptionsPercent;
            default: break;
        }
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
        switch (this.formatter) {
            case 'Integer':
            case 'Long':
            case 'Money':
            case 'Percent':
                this.constraint = new RegExp('\\d');
                break;
            case 'Double':
                this.constraint = new RegExp('[-+]?[0-9]*\.?[0-9]+');
                break;
            default: break;
        }
        if (!this.constraint.test(this.model.value)) {
            this.errorMessage = 'Input not in correct form';
            this.showError = 'inputError';
        }
        else {
            this.errorMessage = '';
            this.showError = '';
        }
        this.eventData.change.emit(this.cmpId);
        this.blur.emit(this);
    }
}
NumericTextBoxComponent.decorators = [
    { type: Component, args: [{
                selector: 'tb-numeric-text-box',
                template: "<div class=\"tb-control tb-numeric-text-box\"> <tb-caption caption=\"{{caption}}\" [for]=\"cmpId\"></tb-caption> <kendo-numerictextbox [ngClass]=\"showError\" [(ngModel)]=\"selectedValue\" (blur)=\"onBlur()\" (ngModelChange)=\"model.value=$event\" [spinners]=\"false\" [format]=\"getFormatOptions()\" [decimals]=\"getDecimalsOptions()\" [disabled]=\"!model?.enabled\" [style.width.px]=\"width\"></kendo-numerictextbox> <div class=\"has-error\">{{errorMessage}}</div> </div>",
                styles: [""]
            },] },
];
/**
 * @nocollapse
 */
NumericTextBoxComponent.ctorParameters = () => [
    { type: EventDataService, },
];
NumericTextBoxComponent.propDecorators = {
    'forCmpID': [{ type: Input },],
    'formatter': [{ type: Input },],
    'disabled': [{ type: Input },],
    'decimals': [{ type: Input },],
    'width': [{ type: Input },],
};
function NumericTextBoxComponent_tsickle_Closure_declarations() {
    /** @type {?} */
    NumericTextBoxComponent.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    NumericTextBoxComponent.ctorParameters;
    /** @type {?} */
    NumericTextBoxComponent.propDecorators;
    /** @type {?} */
    NumericTextBoxComponent.prototype.forCmpID;
    /** @type {?} */
    NumericTextBoxComponent.prototype.formatter;
    /** @type {?} */
    NumericTextBoxComponent.prototype.disabled;
    /** @type {?} */
    NumericTextBoxComponent.prototype.decimals;
    /** @type {?} */
    NumericTextBoxComponent.prototype.width;
    /** @type {?} */
    NumericTextBoxComponent.prototype.errorMessage;
    /** @type {?} */
    NumericTextBoxComponent.prototype.constraint;
    /** @type {?} */
    NumericTextBoxComponent.prototype.selectedValue;
    /** @type {?} */
    NumericTextBoxComponent.prototype.showError;
    /** @type {?} */
    NumericTextBoxComponent.prototype.formatOptionsCurrency;
    /** @type {?} */
    NumericTextBoxComponent.prototype.formatOptionsInteger;
    /** @type {?} */
    NumericTextBoxComponent.prototype.formatOptionsDouble;
    /** @type {?} */
    NumericTextBoxComponent.prototype.formatOptionsPercent;
    /** @type {?} */
    NumericTextBoxComponent.prototype.eventData;
}
