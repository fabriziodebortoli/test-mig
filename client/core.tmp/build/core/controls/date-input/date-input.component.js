import { Component, Input } from '@angular/core';
import { formatDate } from '@progress/kendo-angular-intl';
import { ControlComponent } from './../control.component';
import { EventDataService } from './../../services/eventdata.service';
export class DateInputComponent extends ControlComponent {
    /**
     * @param {?} eventData
     */
    constructor(eventData) {
        super();
        this.eventData = eventData;
        this.readonly = false;
        this.dateFormat = 'dd MMM yyyy';
    }
    /**
     * @param {?} val
     * @return {?}
     */
    onChange(val) {
        this.onUpdateNgModel(val);
    }
    /**
     * @return {?}
     */
    onBlur() {
        this.eventData.change.emit(this.cmpId);
        this.blur.emit(this);
    }
    /**
     * @param {?} newDate
     * @return {?}
     */
    onUpdateNgModel(newDate) {
        if (!newDate) {
            return;
        }
        const /** @type {?} */ timestamp = Date.parse(newDate.toDateString());
        if (isNaN(timestamp)) {
            return;
        }
        if (!this.modelValid()) {
            this.model = { enable: 'true', value: '' };
        }
        this.selectedDate = newDate;
        this.model.value = formatDate(this.selectedDate, 'y-MM-ddTHH:mm:ss');
    }
    /**
     * @return {?}
     */
    ngAfterViewInit() {
        if (this.modelValid()) {
            this.onUpdateNgModel(new Date(this.model.value));
        }
    }
    /**
     * @return {?}
     */
    ngOnChanges() {
        if (this.modelValid()) {
            this.onUpdateNgModel(new Date(this.model.value));
        }
    }
    /**
     * @param {?} formatter
     * @return {?}
     */
    getFormat(formatter) {
        switch (formatter) {
            case 'Date':
                this.dateFormat = 'dd MMM yyyy';
                break;
            case 'DateTime':
                this.dateFormat = 'dd MMM yyyy HH:mm';
                break;
            case 'DateTimeExtended':
                this.dateFormat = 'dd MMM yyyy HH:mm:ss';
                break;
            default: break;
        }
        return this.dateFormat;
    }
    /**
     * @return {?}
     */
    modelValid() {
        return this.model !== undefined && this.model !== null;
    }
}
DateInputComponent.decorators = [
    { type: Component, args: [{
                selector: 'tb-date-input',
                template: "<div class=\"tb-control tb-date-input\"> <tb-caption caption=\"{{caption}}\" [for]=\"cmpId\"></tb-caption> <kendo-datepicker [(ngModel)]=\"selectedDate\" [format]=\"getFormat(formatter)\" (valueChange)=\"onChange($event)\" [focusedDate]=\"selectedDate\" [disabled]=\"!model?.enable\" (blur)=\"onBlur($event)\"> </kendo-datepicker> </div>",
                styles: [""]
            },] },
];
/**
 * @nocollapse
 */
DateInputComponent.ctorParameters = () => [
    { type: EventDataService, },
];
DateInputComponent.propDecorators = {
    'forCmpID': [{ type: Input },],
    'formatter': [{ type: Input },],
    'readonly': [{ type: Input },],
};
function DateInputComponent_tsickle_Closure_declarations() {
    /** @type {?} */
    DateInputComponent.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    DateInputComponent.ctorParameters;
    /** @type {?} */
    DateInputComponent.propDecorators;
    /** @type {?} */
    DateInputComponent.prototype.forCmpID;
    /** @type {?} */
    DateInputComponent.prototype.formatter;
    /** @type {?} */
    DateInputComponent.prototype.readonly;
    /** @type {?} */
    DateInputComponent.prototype.selectedDate;
    /** @type {?} */
    DateInputComponent.prototype.dateFormat;
    /** @type {?} */
    DateInputComponent.prototype.eventData;
}
