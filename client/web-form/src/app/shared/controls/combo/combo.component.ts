import { ControlComponent } from './../control.component';
import { Component, Input, OnChanges, OnInit, AfterViewInit } from '@angular/core';
import { EnumsService } from './../../../core/enums.service';
import { EventDataService } from './../../../core/eventdata.service';
import { DocumentService } from './../../../core/document.service';
import { WebSocketService } from './../../../core/websocket.service';

@Component({
    selector: 'tb-combo',
    templateUrl: 'combo.component.html',
    styleUrls: ['combo.component.scss']
})

export class ComboComponent extends ControlComponent implements OnChanges {

    private items: Array<any> = [];
    private selectedItem: any;

    @Input() public itemSource: any;
    constructor(
        private webSocketService: WebSocketService,
        private eventDataService: EventDataService
        ) {
        super();

        this.webSocketService.itemSource.subscribe((result) => {
            this.items = result.itemSource;
        });
    }

    fillListBox() {
        this.items.splice(0, this.items.length);
        this.eventDataService.openDropdown.emit(this.itemSource);
    }

    onChange() {
        console.log(this.selectedItem);
    }

    ngOnChanges(changes: {}) {
        if (changes['model'] == undefined || changes['model'].currentValue == undefined) return;

        this.items.splice(0, this.items.length);
        let temp = changes['model'].currentValue.value;

        let obj = { code: temp, description: temp };
        this.items.push(obj);
        this.selectedItem = obj;
    }
}
