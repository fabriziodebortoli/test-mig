import { Component, Input, OnChanges, DoCheck, OnDestroy } from '@angular/core';

import { Subscription } from 'rxjs';

import { ControlComponent } from './../control.component';

import { EventDataService } from './../../services/eventdata.service';
import { WebSocketService } from './../../services/websocket.service';

@Component({
    selector: 'tb-combo',
    template: "<!--<tb-caption caption=\"{{caption}}\"></tb-caption> <kendo-dropdownlist id=\"{{cmpId}}\" [data]=\"items\" [textField]=\"'description'\" [valueField]=\"'code'\" [value]=\"selectedItem\" (selectionChange)=\"onChange()\" (open)=\"fillListBox()\"> <template kendoDropDownListNoDataTemplate> <h4><span class=\"k-icon k-i-warning\"></span><br /><br /> No data here</h4> </template> </kendo-dropdownlist>--> <div class=\"tb-control tb-combo\"> <tb-caption caption=\"{{caption}}\" [for]=\"cmpId\"></tb-caption> <kendo-dropdownlist [data]=\"items\" [textField]=\"'description'\" [disabled]=\"!model?.enabled\" [valueField]=\"'code'\" [value]=\"selectedItem\" (selectionChange)=\"onChange($event)\" (open)=\"fillListBox()\" [popupSettings]=\"{ height: 300, width: 400 }\" [style.width.px]=\"width\"> <template kendoDropDownListNoDataTemplate> <h4><span class=\"k-icon k-i-warning\"></span><br /><br /> No data here</h4> </template> </kendo-dropdownlist> </div>",
    styles: [".k-i-warning { font-size: 2.5em; } h4 { font-size: 1em; } "]
})
export class ComboComponent extends ControlComponent implements OnChanges, DoCheck, OnDestroy {

    private items: Array<any> = [];
    private selectedItem: any;
    private itemSourceSub: Subscription;
    @Input() public itemSource: any = undefined;
    @Input() public hotLink: any = undefined;
    @Input() width: number;

    constructor(
        private webSocketService: WebSocketService,
        private eventDataService: EventDataService
    ) {
        super();

        this.itemSourceSub = this.webSocketService.itemSource.subscribe((result) => {
            this.items = result.itemSource;
        });
    }

    fillListBox() {
        this.items.splice(0, this.items.length);


        this.eventDataService.openDropdown.emit(this);
    }

    onChange(change: any) {
        if (this.model.value == change.code)
            return;

        this.selectedItem = change;
        this.model.value = this.selectedItem.code;
    }

    ngDoCheck() {

        if (this.selectedItem == undefined || this.model == undefined) {
            return;
        }


        if (this.model.value == this.selectedItem.code) {
            return;
        }

        //if (changes['model'] == undefined || changes['model'].currentValue == undefined) return;

        this.items.splice(0, this.items.length);
        let temp = this.model.value;

        let obj = { code: temp, description: temp };
        this.items.push(obj);
        this.selectedItem = obj;
    }

    ngOnChanges(changes: {}) {
        if (changes['model'] == undefined || changes['model'].currentValue == undefined) return;

        this.items.splice(0, this.items.length);
        let temp = changes['model'].currentValue.value;

        let obj = { code: temp, description: temp };
        this.items.push(obj);
        this.selectedItem = obj;
    }

    ngOnDestroy() {
        this.itemSourceSub.unsubscribe();
    }

}
