import { Component } from '@angular/core';
import { ControlTypes } from '../../shared/models/control-types.enum';
import { EventDataService } from '../services/eventdata.service';
import { BOService } from '../services/bo.service';
import { DocumentComponent } from './document.component';
/**
 * @abstract
 */
export class BOComponent extends DocumentComponent {
    /**
     * @param {?} bo
     * @param {?} eventData
     */
    constructor(bo, eventData) {
        super(bo, eventData);
        this.bo = bo;
        this.controlTypeModel = ControlTypes;
    }
    /**
     * @return {?}
     */
    ngOnInit() {
        super.ngOnInit();
    }
    /**
     * @return {?}
     */
    ngOnDestroy() {
        this.bo.dispose();
    }
}
BOComponent.decorators = [
    { type: Component, args: [{
                selector: 'tb-bo',
                template: '',
                styles: []
            },] },
];
/**
 * @nocollapse
 */
BOComponent.ctorParameters = () => [
    { type: BOService, },
    { type: EventDataService, },
];
function BOComponent_tsickle_Closure_declarations() {
    /** @type {?} */
    BOComponent.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    BOComponent.ctorParameters;
    /** @type {?} */
    BOComponent.prototype.controlTypeModel;
    /** @type {?} */
    BOComponent.prototype.bo;
}
