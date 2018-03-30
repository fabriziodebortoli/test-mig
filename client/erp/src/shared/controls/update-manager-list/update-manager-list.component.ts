import { Component, OnInit, Input, ChangeDetectorRef, AfterViewInit } from '@angular/core';

import {
    EventDataService, LayoutService, TbComponentService, ControlComponent, Store, Selector, createSelector, FormMode, WebSocketService
} from '@taskbuilder/core';
import { Subject } from 'rxjs/Subject';

// import { untilDestroy } from './../../../rxjs.imports'
import { untilDestroy } from '@taskbuilder/core/shared/commons/untilDestroy';

type UMErpAction = { index: number, value: boolean, enabled: boolean, description: string };

@Component({
    selector: "erp-update-manager-list",
    templateUrl: './update-manager-list.component.html',
    styleUrls: ['./update-manager-list.component.scss']
})
export class UpdateManagerListComponent extends ControlComponent implements OnInit {

    umActions$ = new Subject<Array<UMErpAction>>();
    _umActions: Array<UMErpAction>;

    action = "";

    @Input() public itemSource: any;

    ngOnInit() {
        // 
        this.webSocketService.itemSourceExtended.pipe(untilDestroy(this)).filter(x => x.cmpId === this.cmpId).subscribe(
            (data) => this.umActions = this.itemSourceToAction(data)
        );
    }

    ngAfterViewInit() {
        if (this.itemSource) {
            this.action = 'getList';
            this.eventData.checkListBoxAction.emit(this);
        }
    }

    itemSourceToAction(data: any): Array<UMErpAction> {
        let ret: Array<UMErpAction> = new Array<UMErpAction>();

        if (data.itemSource && data.itemSource.length) {
            for (let i = 0; i < data.itemSource.length; i++) {
                let erpAction: UMErpAction = {
                    index: i,
                    value: data.itemSource[i].value === 'TRUE' ? true : false,
                    enabled: data.itemSource[i].enabled === 'TRUE' ? true : false,
                    description: data.itemSource[i].description
                };
                ret.push(erpAction);
            }
        }

        return ret;
    }

    get umActions(): Array<UMErpAction> {
        return this._umActions;
    }
    set umActions(value: Array<UMErpAction>) {
        this._umActions = value;
        this.umActions$.next(this._umActions);
    }

    constructor(
        public eventData: EventDataService,
        layoutService: LayoutService,
        tbComponentService: TbComponentService,
        changeDetectorRef: ChangeDetectorRef,
        private webSocketService: WebSocketService
    ) {
        super(layoutService, tbComponentService, changeDetectorRef);
    }

    valueChanged(event) {
        this.model.value = event;
        this.eventData.change.emit(this.cmpId);
    }
}