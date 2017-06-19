import { Component, Input, ViewChild, ViewContainerRef, ComponentFactoryResolver } from '@angular/core';
import { ControlComponent } from '../control.component';
import { EventDataService } from './../../services/eventdata.service';
export class TextComponent extends ControlComponent /*implements AfterContentInit, OnChanges */ {
    /**
     * @param {?} eventData
     * @param {?} vcr
     * @param {?} componentResolver
     */
    constructor(eventData, vcr, componentResolver) {
        super();
        this.eventData = eventData;
        this.vcr = vcr;
        this.componentResolver = componentResolver;
        this.readonly = false;
        this.hotLink = undefined;
    }
    /**
     * @return {?}
     */
    onBlur() {
        this.eventData.change.emit(this.cmpId);
        this.blur.emit(this);
    }
}
TextComponent.decorators = [
    { type: Component, args: [{
                selector: 'tb-text',
                template: "<div class=\"tb-control tb-text\"> <tb-caption caption=\"{{caption}}\" forCmpID=\"forCmpID\"></tb-caption> <div class=\"group\"> <kendo-maskedtextbox [mask]=\"mask\" [ngModel]=\"model?.value\" required=\"required\" (blur)=\"onBlur()\" [disabled]=\"!(model?.enabled)\" (ngModelChange)=\"model.value=$event\" [style.width.px]=\"width\"></kendo-maskedtextbox> <ng-container #contextMenu></ng-container> </div> </div>",
                styles: [""]
            },] },
];
/**
 * @nocollapse
 */
TextComponent.ctorParameters = () => [
    { type: EventDataService, },
    { type: ViewContainerRef, },
    { type: ComponentFactoryResolver, },
];
TextComponent.propDecorators = {
    'readonly': [{ type: Input, args: ['readonly',] },],
    'hotLink': [{ type: Input },],
    'width': [{ type: Input },],
    'contextMenu': [{ type: ViewChild, args: ['contextMenu', { read: ViewContainerRef },] },],
};
function TextComponent_tsickle_Closure_declarations() {
    /** @type {?} */
    TextComponent.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    TextComponent.ctorParameters;
    /** @type {?} */
    TextComponent.propDecorators;
    /** @type {?} */
    TextComponent.prototype.readonly;
    /** @type {?} */
    TextComponent.prototype.hotLink;
    /** @type {?} */
    TextComponent.prototype.width;
    /** @type {?} */
    TextComponent.prototype.contextMenu;
    /** @type {?} */
    TextComponent.prototype.eventData;
    /** @type {?} */
    TextComponent.prototype.vcr;
    /** @type {?} */
    TextComponent.prototype.componentResolver;
}
