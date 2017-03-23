import { ControlComponent } from './../control.component';
import { Component, Input, OnInit } from '@angular/core';
import { EventDataService } from './../../../core/eventdata.service';
import { DocumentService } from './../../../core/document.service';
import { WebSocketService } from './../../../core/websocket.service';

@Component({
    selector: 'tb-combo',
    templateUrl: 'combo.component.html',
    styleUrls: ['./combo.component.scss']
})

export class ComboComponent extends ControlComponent {

    @Input()
    itemSourceName: string;
    @Input()
    itemSourceNamespace: string;
    @Input()
    itemSourceParameter: string;

    private items: any;
    private selectedItem: string;

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
        let obj = {itemSourceName: this.itemSourceName, itemSourceNamespace: this.itemSourceNamespace, itemSourceParameter: this.itemSourceParameter};
        this.eventDataService.openDropdown.emit(obj);        
    }

    onChange() {
        //this.selectedValue = $event;
        console.log(this.selectedItem);
    }
}
