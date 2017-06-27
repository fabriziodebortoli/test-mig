import { Component, Input } from '@angular/core';
import { ControlComponent } from './../control.component';
import { EventDataService } from './../../services/eventdata.service';
import { WebSocketService } from './../../services/websocket.service';
export class ComboComponent extends ControlComponent {
    /**
     * @param {?} webSocketService
     * @param {?} eventDataService
     */
    constructor(webSocketService, eventDataService) {
        super();
        this.webSocketService = webSocketService;
        this.eventDataService = eventDataService;
        this.items = [];
        this.itemSource = undefined;
        this.hotLink = undefined;
        this.itemSourceSub = this.webSocketService.itemSource.subscribe((result) => {
            this.items = result.itemSource;
        });
    }
    /**
     * @return {?}
     */
    fillListBox() {
        this.items.splice(0, this.items.length);
        this.eventDataService.openDropdown.emit(this);
    }
    /**
     * @param {?} change
     * @return {?}
     */
    onChange(change) {
        if (this.model.value == change.code)
            return;
        this.selectedItem = change;
        this.model.value = this.selectedItem.code;
    }
    /**
     * @return {?}
     */
    ngDoCheck() {
        if (this.selectedItem == undefined || this.model == undefined) {
            return;
        }
        if (this.model.value == this.selectedItem.code) {
            return;
        }
        //if (changes['model'] == undefined || changes['model'].currentValue == undefined) return;
        this.items.splice(0, this.items.length);
        let /** @type {?} */ temp = this.model.value;
        let /** @type {?} */ obj = { code: temp, description: temp };
        this.items.push(obj);
        this.selectedItem = obj;
    }
    /**
     * @param {?} changes
     * @return {?}
     */
    ngOnChanges(changes) {
        if (changes['model'] == undefined || changes['model'].currentValue == undefined)
            return;
        this.items.splice(0, this.items.length);
        let /** @type {?} */ temp = changes['model'].currentValue.value;
        let /** @type {?} */ obj = { code: temp, description: temp };
        this.items.push(obj);
        this.selectedItem = obj;
    }
    /**
     * @return {?}
     */
    ngOnDestroy() {
        this.itemSourceSub.unsubscribe();
    }
}
ComboComponent.decorators = [
    { type: Component, args: [{
                selector: 'tb-combo',
                template: "<!--<tb-caption caption=\"{{caption}}\"></tb-caption> <kendo-dropdownlist id=\"{{cmpId}}\" [data]=\"items\" [textField]=\"'description'\" [valueField]=\"'code'\" [value]=\"selectedItem\" (selectionChange)=\"onChange()\" (open)=\"fillListBox()\"> <template kendoDropDownListNoDataTemplate> <h4><span class=\"k-icon k-i-warning\"></span><br /><br /> No data here</h4> </template> </kendo-dropdownlist>--> <div class=\"tb-control tb-combo\"> <tb-caption caption=\"{{caption}}\" [for]=\"cmpId\"></tb-caption> <kendo-dropdownlist [data]=\"items\" [textField]=\"'description'\" [disabled]=\"!model?.enabled\" [valueField]=\"'code'\" [value]=\"selectedItem\" (selectionChange)=\"onChange($event)\" (open)=\"fillListBox()\" [popupSettings]=\"{ height: 300, width: 400 }\" [style.width.px]=\"width\"> <template kendoDropDownListNoDataTemplate> <h4><span class=\"k-icon k-i-warning\"></span><br /><br /> No data here</h4> </template> </kendo-dropdownlist> </div>",
                styles: [".k-i-warning { font-size: 2.5em; } h4 { font-size: 1em; } "]
            },] },
];
/**
 * @nocollapse
 */
ComboComponent.ctorParameters = () => [
    { type: WebSocketService, },
    { type: EventDataService, },
];
ComboComponent.propDecorators = {
    'itemSource': [{ type: Input },],
    'hotLink': [{ type: Input },],
    'width': [{ type: Input },],
};
function ComboComponent_tsickle_Closure_declarations() {
    /** @type {?} */
    ComboComponent.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    ComboComponent.ctorParameters;
    /** @type {?} */
    ComboComponent.propDecorators;
    /** @type {?} */
    ComboComponent.prototype.items;
    /** @type {?} */
    ComboComponent.prototype.selectedItem;
    /** @type {?} */
    ComboComponent.prototype.itemSourceSub;
    /** @type {?} */
    ComboComponent.prototype.itemSource;
    /** @type {?} */
    ComboComponent.prototype.hotLink;
    /** @type {?} */
    ComboComponent.prototype.width;
    /** @type {?} */
    ComboComponent.prototype.webSocketService;
    /** @type {?} */
    ComboComponent.prototype.eventDataService;
}
