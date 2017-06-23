import { Subscription } from 'rxjs';
import { ControlComponent } from './../control.component';
import { Component, Input, OnChanges, OnInit, AfterViewInit, DoCheck, OnDestroy } from '@angular/core';
import { EnumsService } from './../../../core/enums.service';
import { EventDataService } from './../../../core/eventdata.service';
import { DocumentService } from '@taskbuilder/core';
import { WebSocketService } from '@taskbuilder/core';

@Component({
    selector: 'tb-combo',
    templateUrl: 'combo.component.html',
    styleUrls: ['combo.component.scss']
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
