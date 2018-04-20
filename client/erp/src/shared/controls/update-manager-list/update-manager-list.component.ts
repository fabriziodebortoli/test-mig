import { Component, OnInit, Input, ChangeDetectorRef, AfterViewInit, HostListener } from '@angular/core';

import {
    EventDataService, LayoutService, TbComponentService, ControlComponent, Store, Selector, createSelector, FormMode, WebSocketService
} from '@taskbuilder/core';
import { Subject } from 'rxjs/Subject';

// import { untilDestroy } from './../../../rxjs.imports'
import { untilDestroy } from '@taskbuilder/core/shared/commons/untilDestroy';

type UMErpAction = { index: number, value: boolean, enabled: boolean, description: string };

enum CheckListBoxAction { CLB_QUERY_LIST = 0, CLB_SET_VALUES = 1, CLB_DBL_CLICK = 2 };

@Component({
    selector: "erp-update-manager-list",
    templateUrl: './update-manager-list.component.html',
    styleUrls: ['./update-manager-list.component.scss']
})
export class UpdateManagerListComponent extends ControlComponent implements OnInit {

    umActions$ = new Subject<Array<UMErpAction>>();
    _umActions: Array<UMErpAction>;
    currentIndex = -1;

    private _itemSourceName: any = undefined;
    @Input()
    set itemSourceName(itemSourceName: any) {
        this._itemSourceName = itemSourceName instanceof Object ? itemSourceName.value : itemSourceName;
    }
  
    get itemSourceName() {
        return this._itemSourceName;
    }

    private _itemSourceNamespace: any = undefined;
    @Input()
    set itemSourceNamespace(itemSourceNamespace: any) {
        this._itemSourceNamespace = itemSourceNamespace instanceof Object ? itemSourceNamespace.value : itemSourceNamespace;
    }
  
    get itemSourceNamespace() {
        return this._itemSourceNamespace;
    }


    ngOnInit() {
        // 
        this.webSocketService.itemSourceExtended.pipe(untilDestroy(this)).filter(x => x.cmpId === this.cmpId).subscribe(
            (data) => this.umActions = this.itemSourceToAction(data)
        );
    }

    ngAfterViewInit() {
        if (this.itemSourceName && this.itemSourceNamespace) {
            this.eventData.checkListBoxAction.emit(this.getActionObject(CheckListBoxAction.CLB_QUERY_LIST));
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
        this._umActions[this.currentIndex].value = event;
        this.eventData.checkListBoxAction.emit(this.getActionObject(CheckListBoxAction.CLB_SET_VALUES, this.currentIndex));
    }

    @HostListener('dblclick', ['$event'])
    onDblClick(event: any) {
        this.eventData.checkListBoxAction.emit(this.getActionObject(CheckListBoxAction.CLB_DBL_CLICK, this.currentIndex));
    }

    onClick(event) {
        if (event && event.target)
            this.currentIndex = +event.target.id;
    }

    getActionObject(action: CheckListBoxAction, itemID: number = 0): any {
        //  let jsonCheckList = JSON.stringify(this._umActions);
        return {
            list: this._umActions,
            action: action,
            cmpId: this.cmpId,
            itemSourceName: this.itemSourceName,
            itemSourceNamespace: this.itemSourceNamespace,
            itemID: itemID
        }
    }
}