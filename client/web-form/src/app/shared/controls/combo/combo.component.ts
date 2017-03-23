import { AfterViewInit } from 'libclient/node_modules/@angular/core';
import { ControlComponent } from './../control.component';
import { Component, Input, OnInit, OnChanges } from '@angular/core';
import { EventDataService } from './../../../core/eventdata.service';
import { DocumentService } from './../../../core/document.service';
import { WebSocketService } from './../../../core/websocket.service';

@Component({
    selector: 'tb-combo',
    templateUrl: 'combo.component.html',
    styleUrls: ['./combo.component.scss']
})

export class ComboComponent extends ControlComponent implements AfterViewInit, OnChanges {

    @Input()
    itemSourceName: string;
    @Input()
    itemSourceNamespace: string;
    @Input()
    itemSourceParameter: string;

    private items: Array<any> = [];
    private selectedItem: any;

    constructor(
        private webSocketService: WebSocketService,
        private eventDataService: EventDataService) {
        super();

        this.webSocketService.itemSource.subscribe((result) => {
            console.log(this.model);
            this.items = result.itemSource;
        });
    }

    fillListBox() {
        this.items.slice(0, this.items.length);
        let obj = { itemSourceName: this.itemSourceName, itemSourceNamespace: this.itemSourceNamespace, itemSourceParameter: this.itemSourceParameter };
        this.eventDataService.openDropdown.emit(obj);
    }

    onChange() {
        //this.selectedValue = $event;
        console.log(this.selectedItem);
    }

    ngAfterViewInit() {
        console.log("ciao");
        if (this.model != undefined)
            console.log(this.model.value);

    }

    ngOnChanges(changes: {}) {
        if (changes['model'] == undefined || changes['model'].currentValue == undefined)
            return;

        this.items.splice(0, this.items.length);
        let temp = changes['model'].currentValue.value;
        let obj = { code: temp, description: temp };
        this.items.push(obj);
        console.log( this.items);
        this.selectedItem = obj;
    }
}
