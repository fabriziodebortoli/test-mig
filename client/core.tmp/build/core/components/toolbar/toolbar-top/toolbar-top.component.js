import { Component, Input } from '@angular/core';
import { EventDataService } from './../../../services/eventdata.service';
import { ViewModeType } from '../../../../shared/index';
export class ToolbarTopComponent {
    /**
     * @param {?} eventData
     */
    constructor(eventData) {
        this.eventData = eventData;
        this.title = '...';
        this.viewModeTypeModel = ViewModeType;
    }
}
ToolbarTopComponent.decorators = [
    { type: Component, args: [{
                selector: 'tb-toolbar-top',
                template: "<div class=\"toolbar-top\"> <div class=\"toolbar-top-item title\"> <span class=\"document-title truncate\">{{eventData?.model?.Title?.value || title}}</span> </div> <div class=\"toolbar-top-item functions\"></div> <div class=\"toolbar-top-item navigation\"> <ng-content></ng-content> </div> </div>",
                styles: [".toolbar-top { display: flex; flex-direction: row; flex-wrap: nowrap; justify-content: space-between; align-items: stretch; background: #fff; border-bottom: 1px solid #ddd; height: 30px; } .title { flex-grow: 2; display: flex; } .navigation { display: flex; } .document-title { line-height: 32px; margin: 0 10px; font-weight: 700; font-size: 14px; text-transform: uppercase; } "]
            },] },
];
/**
 * @nocollapse
 */
ToolbarTopComponent.ctorParameters = () => [
    { type: EventDataService, },
];
ToolbarTopComponent.propDecorators = {
    'title': [{ type: Input },],
};
function ToolbarTopComponent_tsickle_Closure_declarations() {
    /** @type {?} */
    ToolbarTopComponent.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    ToolbarTopComponent.ctorParameters;
    /** @type {?} */
    ToolbarTopComponent.propDecorators;
    /** @type {?} */
    ToolbarTopComponent.prototype.title;
    /** @type {?} */
    ToolbarTopComponent.prototype.viewModeTypeModel;
    /** @type {?} */
    ToolbarTopComponent.prototype.eventData;
}
