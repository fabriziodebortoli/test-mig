import { Component, Input } from '@angular/core';
import { ControlComponent } from './../../control.component';
import { EventDataService } from './../../../services/eventdata.service';
const /** @type {?} */ DEFAULT_MAX_RANGE = 10;
export class LinearGaugeComponent extends ControlComponent {
    /**
     * @param {?} eventData
     */
    constructor(eventData) {
        super();
        this.eventData = eventData;
        this.setDefault();
    }
    /**
     * @return {?}
     */
    ngOnInit() {
        if (this.maxRange == undefined) {
            this.maxRange = DEFAULT_MAX_RANGE;
        }
        this.rulerAxis = {
            min: 0,
            max: this.maxRange,
            plotBands: [{
                    from: 0, to: this.maxRange, color: this.bandColor, opacity: this.bandOpacity
                }]
        };
    }
    /**
     * @return {?}
     */
    setDefault() {
        this.bandColor = "#f0f0f0";
        this.bandOpacity = 1;
    }
    /**
     * @return {?}
     */
    onBlur() {
        this.eventData.change.emit(this.cmpId);
    }
}
LinearGaugeComponent.decorators = [
    { type: Component, args: [{
                selector: 'tb-linear-gauge',
                template: "<kendo-sparkline [data]=\"model?.nCurrentElement\" type=\"bullet\" [valueAxis]=\"rulerAxis\" (blur)=\"onBlur($event)\"></kendo-sparkline>",
                styles: [""]
            },] },
];
/**
 * @nocollapse
 */
LinearGaugeComponent.ctorParameters = () => [
    { type: EventDataService, },
];
LinearGaugeComponent.propDecorators = {
    'maxRange': [{ type: Input },],
};
function LinearGaugeComponent_tsickle_Closure_declarations() {
    /** @type {?} */
    LinearGaugeComponent.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    LinearGaugeComponent.ctorParameters;
    /** @type {?} */
    LinearGaugeComponent.propDecorators;
    /** @type {?} */
    LinearGaugeComponent.prototype.maxRange;
    /** @type {?} */
    LinearGaugeComponent.prototype.bandColor;
    /** @type {?} */
    LinearGaugeComponent.prototype.bandOpacity;
    /** @type {?} */
    LinearGaugeComponent.prototype.rulerAxis;
    /** @type {?} */
    LinearGaugeComponent.prototype.eventData;
}
