import { EnumsService } from './../../../core/enums.service';
import { ControlComponent } from './../control.component';
import { Component, Input, OnInit, OnChanges, AfterViewInit, DoCheck } from '@angular/core';
import { EventDataService } from './../../../core/eventdata.service';
import { DocumentService } from './../../../core/document.service';
import { WebSocketService } from './../../../core/websocket.service';

@Component({
    selector: 'tb-enum-combo',
    templateUrl: 'enum-combo.component.html',
    styleUrls: ['./enum-combo.component.scss']
})

export class EnumComboComponent extends ControlComponent implements OnChanges, DoCheck {

    private tag: string;

    private items: Array<any> = [];
    private selectedItem: any;

    @Input()
    public itemSource: any;

    constructor(
        private webSocketService: WebSocketService,
        private eventDataService: EventDataService,
        private enumsService: EnumsService) {
        super();

        this.webSocketService.itemSource.subscribe((result) => {
            this.items = result.itemSource;
        });
    }

    fillListBox() {
        this.items.splice(0, this.items.length);

        if (this.itemSource != undefined) {
            this.eventDataService.openDropdown.emit(this.itemSource);
        }
        else {
            let allItems = this.enumsService.getItemsFromTag(this.tag);
            for (let index = 0; index < allItems.length; index++) {
                this.items.push({ code: allItems[index].value, description: allItems[index].name });
            }

        }
    }

    onChange() {
        console.log(this.selectedItem);
    }

    ngDoCheck() {

        //TODOLUCA, è inefficiente, perchè viene chiamato un sacco di volte, ma il model con il jsonpatch non mi viene più cambiato
        if (this.selectedItem == undefined || this.model == undefined)
            return;


        this.tag = this.model.tag;
        let temp = this.model.value;

        let enumItem = this.enumsService.getEnumsItem(temp);
        if (enumItem != undefined)
            temp = enumItem.name;

        if (temp == this.selectedItem.code)
            return;

        this.items.splice(0, this.items.length);

        let obj = { code: temp, description: temp };
        this.items.push(obj);
        this.selectedItem = obj;
    }

    ngOnChanges(changes: {}) {
        if (changes['model'] == undefined || changes['model'].currentValue == undefined)
            return;

        if (this.model.type != 10) {
            console.log("wrong databinding, not a data enum");
        }

        this.tag = this.model.tag;
        let temp = changes['model'].currentValue.value;

        let enumItem = this.enumsService.getEnumsItem(temp);
        if (enumItem != undefined)
            temp = enumItem.name;

        this.items.splice(0, this.items.length);
    
        let obj = { code: temp, description: temp };
        this.items.push(obj);
        this.selectedItem = obj;
    }
}
