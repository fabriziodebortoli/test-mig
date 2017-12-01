import { TbComponentService } from './../../../core/services/tbcomponent.service';
import { LayoutService } from './../../../core/services/layout.service';
import { Component, Input, OnChanges, OnInit, AfterViewInit, DoCheck, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { Subscription } from '../../../rxjs.imports';

import { EventDataService } from './../../../core/services/eventdata.service';
import { WebSocketService } from './../../../core/services/websocket.service';

import { ControlComponent } from './../control.component';


@Component({
    selector: 'tb-combo',
    templateUrl: 'combo.component.html',
    styleUrls: ['combo.component.scss']
})

export class ComboComponent extends ControlComponent implements OnChanges, DoCheck, OnDestroy {

    items: Array<any> = [];
    selectedItem: any;
    public itemSourceSub: Subscription;
    @Input() public itemSource: any = undefined;
    @Input() public hotLink: { namespace: string, name: string };

    ngOnInit() {
        console.log('OnInit ComboComponent Model ' + this.model ? JSON.stringify(this.model) : "niente modello");
    }

    constructor(
        public webSocketService: WebSocketService,
        public eventDataService: EventDataService,
        layoutService: LayoutService,
        tbComponentService: TbComponentService,
        changeDetectorRef: ChangeDetectorRef
    ) {
        super(layoutService, tbComponentService, changeDetectorRef);

        this.itemSourceSub = this.webSocketService.itemSource.subscribe((result) => {
            if (result.itemSource) this.items = result.itemSource;
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
