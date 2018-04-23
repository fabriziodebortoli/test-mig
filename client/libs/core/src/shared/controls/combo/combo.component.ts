import { TbComponentService } from './../../../core/services/tbcomponent.service';
import { LayoutService } from './../../../core/services/layout.service';
import { Component, Input, OnChanges, OnInit, AfterViewInit, DoCheck, OnDestroy, ChangeDetectorRef, ViewChild, HostListener, ChangeDetectionStrategy } from '@angular/core';
import { Subscription, Observable, BehaviorSubject, Subject } from '../../../rxjs.imports';

import { EventDataService } from './../../../core/services/eventdata.service';
import { WebSocketService } from './../../../core/services/websocket.service';

import { ControlComponent } from './../control.component';
import { HotLinkInfo } from './../../models/hotLinkInfo.model';

import { untilDestroy } from '../../commons/untilDestroy';

export type ComboData = { code: any, description: any };

@Component({
    selector: 'tb-combo',
    templateUrl: 'combo.component.html',
    styleUrls: ['combo.component.scss'],
    changeDetection: ChangeDetectionStrategy.Default
})

export class ComboComponent extends ControlComponent implements AfterViewInit, OnChanges, DoCheck, OnDestroy {

    @Input() decimals = 0; //todoluca, serve solo per far compilare, poi sarà da gestire

    items: ComboData[] = [];
    selectedItem: any;
    private oldValue: any;

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

    @Input() public : any = undefined;

    @Input() propagateSelectionChange = false;
    @ViewChild("ddl") public dropdownlist: any;

    //TODOLUCA, qua non dovrebbero proprio esserci, 
    @Input('rows') rows: number = 0;
    @Input('chars') chars: number = 0;

    public isCombo = true;
    private isReady: Observable<boolean>;

    constructor(
        public webSocketService: WebSocketService,
        public eventDataService: EventDataService,
        layoutService: LayoutService,
        tbComponentService: TbComponentService,
        changeDetectorRef: ChangeDetectorRef
    ) {
        super(layoutService, tbComponentService, changeDetectorRef);

        this.isReady = new BehaviorSubject(false).distinctUntilChanged();

        this.webSocketService.itemSource.pipe(untilDestroy(this)).filter(x => x.cmpId === this.cmpId).subscribe((result) => {
            if (result.itemSource) {
                for (let j = 0; j < result.itemSource.length; j++) {
                    this.items.push(result.itemSource[j]);
                }
                if (this.items.length > 0) this.items.push({ code: '', description: '' });
            };
            if (this.dropdownlist) {
                (this.isReady as Subject<boolean>).next(true);
            }
        });
    }

    ngAfterViewInit() {
        super.ngAfterViewInit();
        if (this.dropdownlist) {
            this.isReady.subscribe(ready => {
                if (ready) {
                    this.dropdownlist.toggle(true);
                    (this.isReady as Subject<boolean>).next(false);
                }
            });
        }
    }

    @HostListener('keydown', ['$event'])
    public keydown(event: any): void {
        if (this.dropdownlist && event.target.id === this.dropdownlist.id) {
            switch (event.keyCode) {
                case 9: // tab
                case 13: // enter
                    this.oldValue = this.selectedItem;
                    break;
                case 27: // esc
                    this.selectedItem = this.oldValue;
                    break;
            }
        }
    }

    fillListBox(event: any) {
        event.preventDefault();
        this.items.splice(0, this.items.length);
        this.eventDataService.openDropdown.emit(this);
    }

    onClose(event: any) {
        if (this.dropdownlist) (this.isReady as Subject<boolean>).next(false);
    }

    valueChange(value: any) {
        if (value) {
            if (this.model.value == value.code) { return; }

            this.selectedItem = value;
            this.model.value = this.selectedItem.code;
            this.eventDataService.change.emit(this.cmpId);
        }
    }

    selectionChange(value: any) {
        if (this.propagateSelectionChange) this.valueChange(value);
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

    focus() {
        this.oldValue = this.selectedItem;
    }

    // valueChange(value) {
    //     this.selectedItem = value;
    // }
}
