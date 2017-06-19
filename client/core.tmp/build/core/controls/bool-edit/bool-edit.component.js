import { Component, Input } from '@angular/core';
import { ControlComponent } from './../control.component';
import { EventDataService } from './../../services/eventdata.service';
export class BoolEditComponent extends ControlComponent {
    /**
     * @param {?} eventData
     */
    constructor(eventData) {
        super();
        this.eventData = eventData;
        if (this.yesText == null ||
            this.noText == null ||
            this.yesText.length == 0 ||
            this.noText.length == 0) {
            this.yesText = 'YES';
            this.noText = 'NO';
        }
    }
    /**
     * @param {?} event
     * @return {?}
     */
    keyPress(event) {
        let /** @type {?} */ firstYes = this.yesText[0].toUpperCase();
        let /** @type {?} */ localizedCodeYes = 'Key' + firstYes;
        let /** @type {?} */ localizedCodeNo = 'Key' + this.noText[0].toUpperCase();
        if (event.code != localizedCodeYes &&
            event.code != localizedCodeNo) {
            return;
        }
        event.preventDefault();
        if (this.model == undefined)
            return;
        this.model.value = event.key.toUpperCase() == firstYes ?
            this.yesText.toUpperCase() : this.noText.toUpperCase();
    }
    /**
     * @return {?}
     */
    onBlur() {
        this.eventData.change.emit(this.cmpId);
    }
}
BoolEditComponent.decorators = [
    { type: Component, args: [{
                selector: 'tb-bool-edit',
                template: "<div> <input id=\"{{cmpId}}\"  type=\"text\"  [ngModel]=\"model?.value\" [disabled]=\"!model?.enabled\" (blur)=\"onBlur()\" (keydown)=\"keyPress($event)\" (ngModelChange)=\"model.value=$event\" /> </div>",
                styles: [""]
            },] },
];
/**
 * @nocollapse
 */
BoolEditComponent.ctorParameters = () => [
    { type: EventDataService, },
];
BoolEditComponent.propDecorators = {
    'yesText': [{ type: Input },],
    'noText': [{ type: Input },],
};
function BoolEditComponent_tsickle_Closure_declarations() {
    /** @type {?} */
    BoolEditComponent.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    BoolEditComponent.ctorParameters;
    /** @type {?} */
    BoolEditComponent.propDecorators;
    /** @type {?} */
    BoolEditComponent.prototype.yesText;
    /** @type {?} */
    BoolEditComponent.prototype.noText;
    /** @type {?} */
    BoolEditComponent.prototype.eventData;
}
