import { Component, Input } from '@angular/core';
import { ControlComponent } from './../control.component';
import { EnumsService } from './../../services/enums.service';
import { EventDataService } from './../../services/eventdata.service';
import { WebSocketService } from './../../services/websocket.service';
export class EnumComboComponent extends ControlComponent {
    /**
     * @param {?} webSocketService
     * @param {?} eventDataService
     * @param {?} enumsService
     */
    constructor(webSocketService, eventDataService, enumsService) {
        super();
        this.webSocketService = webSocketService;
        this.eventDataService = eventDataService;
        this.enumsService = enumsService;
        this.items = [];
        this.itemSourceSub = this.webSocketService.itemSource.subscribe((result) => {
            this.items = result.itemSource;
        });
    }
    /**
     * @return {?}
     */
    fillListBox() {
        this.items.splice(0, this.items.length);
        if (this.itemSource != undefined) {
            this.eventDataService.openDropdown.emit(this);
        }
        else {
            let /** @type {?} */ allItems = this.enumsService.getItemsFromTag(this.tag);
            if (allItems != undefined) {
                for (let /** @type {?} */ index = 0; index < allItems.length; index++) {
                    this.items.push({ code: allItems[index].value, description: allItems[index].name });
                }
            }
        }
    }
    /**
     * @return {?}
     */
    onChange() {
        console.log(this.selectedItem);
    }
    /**
     * @return {?}
     */
    ngDoCheck() {
        //TODOLUCA, è inefficiente, perchè viene chiamato un sacco di volte, ma il model con il jsonpatch non mi viene più cambiato
        if (this.selectedItem == undefined || this.model == undefined)
            return;
        this.tag = this.model.tag;
        let /** @type {?} */ temp = this.model.value;
        let /** @type {?} */ enumItem = this.enumsService.getEnumsItem(temp);
        if (enumItem != undefined)
            temp = enumItem.name;
        if (temp == this.selectedItem.code)
            return;
        this.items.splice(0, this.items.length);
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
        if (this.model.type != 10) {
            console.log("wrong databinding, not a data enum");
        }
        this.tag = this.model.tag;
        let /** @type {?} */ temp = changes['model'].currentValue.value;
        let /** @type {?} */ enumItem = this.enumsService.getEnumsItem(temp);
        if (enumItem != undefined)
            temp = enumItem.name;
        this.items.splice(0, this.items.length);
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
EnumComboComponent.decorators = [
    { type: Component, args: [{
                selector: 'tb-enum-combo',
                template: "<div class=\"tb-control tb-enum-combo\"> <tb-caption caption=\"{{caption}}\" [for]=\"cmpId\"></tb-caption> <kendo-dropdownlist id=\"{{cmpId}}\" [data]=\"items\" [textField]=\"'description'\" [disabled]=\"!model?.enabled\" [valueField]=\"'code'\" [value]=\"selectedItem\" (selectionChange)=\"onChange()\" (open)=\"fillListBox()\" [style.width.px]=\"width\"> </kendo-dropdownlist> </div>",
                styles: [""]
            },] },
];
/**
 * @nocollapse
 */
EnumComboComponent.ctorParameters = () => [
    { type: WebSocketService, },
    { type: EventDataService, },
    { type: EnumsService, },
];
EnumComboComponent.propDecorators = {
    'itemSource': [{ type: Input },],
    'width': [{ type: Input },],
};
function EnumComboComponent_tsickle_Closure_declarations() {
    /** @type {?} */
    EnumComboComponent.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    EnumComboComponent.ctorParameters;
    /** @type {?} */
    EnumComboComponent.propDecorators;
    /** @type {?} */
    EnumComboComponent.prototype.tag;
    /** @type {?} */
    EnumComboComponent.prototype.items;
    /** @type {?} */
    EnumComboComponent.prototype.selectedItem;
    /** @type {?} */
    EnumComboComponent.prototype.itemSourceSub;
    /** @type {?} */
    EnumComboComponent.prototype.itemSource;
    /** @type {?} */
    EnumComboComponent.prototype.width;
    /** @type {?} */
    EnumComboComponent.prototype.webSocketService;
    /** @type {?} */
    EnumComboComponent.prototype.eventDataService;
    /** @type {?} */
    EnumComboComponent.prototype.enumsService;
}
