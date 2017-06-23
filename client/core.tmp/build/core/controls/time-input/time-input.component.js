import { Component, Input } from '@angular/core';
import { ControlComponent } from './../control.component';
import { EventDataService } from './../../services/eventdata.service';
export class TimeInputComponent extends ControlComponent {
    /**
     * @param {?} eventData
     */
    constructor(eventData) {
        super();
        this.eventData = eventData;
        this.selectedTime = 0;
    }
    /**
     * @param {?} val
     * @return {?}
     */
    onChange(val) {
        this.onUpdateNgModel(val);
    }
    /**
     * @param {?} newTime
     * @return {?}
     */
    onUpdateNgModel(newTime) {
        if (!this.modelValid()) {
            this.model = { enable: 'true', value: '' };
        }
        this.selectedTime = newTime;
        let /** @type {?} */ r = new Date(this.selectedTime);
        this.model.value = 60 * (60 * r.getHours() + r.getMinutes()) + r.getSeconds();
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
    onBlur() {
        this.eventData.change.emit(this.cmpId);
    }
    /**
     * @return {?}
     */
    modelValid() {
        return this.model !== undefined && this.model !== null;
    }
}
TimeInputComponent.decorators = [
    { type: Component, args: [{
                selector: 'tb-time-input',
                template: "<div class=\"tb-control tb-time-input\"> <tb-caption caption=\"{{caption}}\" [for]=\"cmpId\"></tb-caption> <kendo-dateinput [(ngModel)]=\"selectedTime\" [format]=\"'HH:mm:ss'\" (valueChange)=\"onChange($event)\" (blur)=\"onBlur()\"></kendo-dateinput> </div>",
                styles: [""]
            },] },
];
/**
 * @nocollapse
 */
TimeInputComponent.ctorParameters = () => [
    { type: EventDataService, },
];
TimeInputComponent.propDecorators = {
    'forCmpID': [{ type: Input },],
    'formatter': [{ type: Input },],
};
function TimeInputComponent_tsickle_Closure_declarations() {
    /** @type {?} */
    TimeInputComponent.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    TimeInputComponent.ctorParameters;
    /** @type {?} */
    TimeInputComponent.propDecorators;
    /** @type {?} */
    TimeInputComponent.prototype.selectedTime;
    /** @type {?} */
    TimeInputComponent.prototype.forCmpID;
    /** @type {?} */
    TimeInputComponent.prototype.formatter;
    /** @type {?} */
    TimeInputComponent.prototype.eventData;
}
