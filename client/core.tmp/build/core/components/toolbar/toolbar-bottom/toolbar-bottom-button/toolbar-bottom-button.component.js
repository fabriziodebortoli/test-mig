import { Component, Input } from '@angular/core';
import { EventDataService } from './../../../../services/eventdata.service';
export class ToolbarBottomButtonComponent {
    /**
     * @param {?} eventData
     */
    constructor(eventData) {
        this.eventData = eventData;
        this.caption = '--unknown--';
        this.cmpId = '';
        this.disabled = false;
    }
    /**
     * @return {?}
     */
    ngOnInit() {
    }
    /**
     * @return {?}
     */
    onCommand() {
        this.eventData.command.emit(this.cmpId);
    }
}
ToolbarBottomButtonComponent.decorators = [
    { type: Component, args: [{
                selector: 'tb-toolbar-bottom-button',
                template: "<button kendoButton title='{{caption}}' (click)='onCommand()' [disabled]=\"disabled\">{{caption}}</button>",
                styles: ["button { cursor: pointer; margin: 0 6px 0 0px; background: #065aad; color: #fff; padding: 0 15px; line-height: 30px; border-radius: 5px; font-size: 14px; font-weight: 300; border: 0; } button:hover { background: #003a73; } "]
            },] },
];
/**
 * @nocollapse
 */
ToolbarBottomButtonComponent.ctorParameters = () => [
    { type: EventDataService, },
];
ToolbarBottomButtonComponent.propDecorators = {
    'caption': [{ type: Input },],
    'cmpId': [{ type: Input },],
    'disabled': [{ type: Input },],
};
function ToolbarBottomButtonComponent_tsickle_Closure_declarations() {
    /** @type {?} */
    ToolbarBottomButtonComponent.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    ToolbarBottomButtonComponent.ctorParameters;
    /** @type {?} */
    ToolbarBottomButtonComponent.propDecorators;
    /** @type {?} */
    ToolbarBottomButtonComponent.prototype.caption;
    /** @type {?} */
    ToolbarBottomButtonComponent.prototype.cmpId;
    /** @type {?} */
    ToolbarBottomButtonComponent.prototype.disabled;
    /** @type {?} */
    ToolbarBottomButtonComponent.prototype.eventData;
}
