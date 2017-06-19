import { Component } from '@angular/core';
import { TbComponent } from './tb.component';
import { DocumentService } from '../services/document.service';
import { EventDataService } from '../services/eventdata.service';
/**
 * @abstract
 */
export class DocumentComponent extends TbComponent {
    /**
     * @param {?} document
     * @param {?} eventData
     */
    constructor(document, eventData) {
        super();
        this.document = document;
        this.eventData = eventData;
    }
    /**
     * @return {?}
     */
    ngOnInit() {
        this.viewModeType = this.document.getViewModeType();
        this.title = this.document.getTitle();
    }
}
DocumentComponent.decorators = [
    { type: Component, args: [{
                selector: 'tb-document',
                template: '',
                styles: []
            },] },
];
/**
 * @nocollapse
 */
DocumentComponent.ctorParameters = () => [
    { type: DocumentService, },
    { type: EventDataService, },
];
function DocumentComponent_tsickle_Closure_declarations() {
    /** @type {?} */
    DocumentComponent.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    DocumentComponent.ctorParameters;
    /** @type {?} */
    DocumentComponent.prototype.viewModeType;
    /** @type {?} */
    DocumentComponent.prototype.title;
    /** @type {?} */
    DocumentComponent.prototype.args;
    /** @type {?} */
    DocumentComponent.prototype.document;
    /** @type {?} */
    DocumentComponent.prototype.eventData;
}
