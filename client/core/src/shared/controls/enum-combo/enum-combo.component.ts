import { Logger } from './../../../core/services/logger.service';
import { TbComponentService } from './../../../core/services/tbcomponent.service';
import { LayoutService } from './../../../core/services/layout.service';
import { Component, Input, OnInit, OnChanges, OnDestroy, ChangeDetectorRef, HostListener, ViewChild, ChangeDetectionStrategy } from '@angular/core';
import { Subscription } from '../../../rxjs.imports';

import { EnumsService } from './../../../core/services/enums.service';
import { EventDataService } from './../../../core/services/eventdata.service';
import { WebSocketService } from './../../../core/services/websocket.service';

import { ControlComponent } from './../control.component';

import { Subject } from 'rxjs/Subject';

import { untilDestroy } from '../../commons/untilDestroy';

type ComboData = { code: any, description: any };
const convertToComboData: (arr: Array<any>) => Array<ComboData> = (arr) => arr.map(x => ({ code: x['value'], description: x['name'] }));
const areEqualsComboData: (left: any, right: any) => boolean = (left, right) => JSON.stringify(left) === JSON.stringify(right);

@Component({
    selector: 'tb-enum-combo',
    templateUrl: 'enum-combo.component.html',
    styleUrls: ['./enum-combo.component.scss'],
    changeDetection: ChangeDetectionStrategy.Default
})
export class EnumComboComponent extends ControlComponent implements OnChanges, OnDestroy, OnInit {

    public tag: string;

    private itemSource$ = this.webSocketService.itemSource.pipe(untilDestroy(this)).filter(x => x.cmpId === this.cmpId).map(result => result.itemSource as Array<ComboData>);
    private modelChangedSubscription: Subscription;

    items$ = new Subject<Array<ComboData>>();
    _items: Array<ComboData>;
    get items(): Array<ComboData> {
        return this._items;
    }
    set items(value: Array<ComboData>) {

        this._items = value;
        this.items$.next(this._items);
    }

    selectedItem: any;
    private oldValue: any;
    @Input() public itemSource: any;
    @Input() propagateSelectionChange = false;
    @ViewChild("ddl") dropdownlist: any;
    private stateButtonDisabled = false;
    translateItemsCodes(itemsArray: Array<ComboData>): Array<ComboData> {
        itemsArray.forEach(element => {
            element.code = (+element.code + (65536 * +this.tag)).toString();
        });
        return itemsArray;
    }

    constructor(
        public webSocketService: WebSocketService,
        public eventDataService: EventDataService,
        public enumsService: EnumsService,
        layoutService: LayoutService,
        tbComponentService: TbComponentService,
        changeDetectorRef: ChangeDetectorRef,
        public logger: Logger
    ) {
        super(layoutService, tbComponentService, changeDetectorRef);
    }

    private withItemSourceLogic(): EnumComboComponent {
        this.itemSource$
            .distinctUntilChanged((left, right) => areEqualsComboData(left, right))
            .subscribe(itemSource => this.items = this.translateItemsCodes(itemSource));
        return this;
    }

    ngOnInit() {
        if (this.itemSource) { this.withItemSourceLogic(); }
    }

    onOpen() {
        if (this.itemSource) { this.eventDataService.openDropdown.emit(this); }
        else { this.items = this.translateItemsCodes(convertToComboData(this.enumsService.getItemsFromTag(this.tag) as Array<any>)); }
    }

    @HostListener('keydown', ['$event'])
    public keydown(event: any): void {
        if (event.target.id === this.dropdownlist.id) {
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

    focus() {
        this.oldValue = this.selectedItem;
    }

    valueChange(value) {
        this.selectedItem = value;
        this.model.value = this.selectedItem.code;
        this.eventDataService.change.emit(this.cmpId);
    }

    selectionChange(value) {
        if (this.propagateSelectionChange) this.valueChange(value);
    }

    ngOnChanges(changes: {}) {
        if (!this.model)
            return;
        if (this.model.type != 10) {
            this.logger.debug("wrong databinding, not a data enum");
        }
        this.tag = this.model.tag;
        if (!this.modelChangedSubscription) {
            this.trySetSelectedItem();
            this.modelChangedSubscription = this.model.modelChanged
                .pipe(untilDestroy(this))
                .subscribe(this.trySetSelectedItem);
        }
    }

    trySetSelectedItem = () => {
        let enumItem = this.enumsService.getEnumsItem(this.model.value);
        let desc = enumItem && enumItem.name || '';
        let obj: Array<ComboData> = [{ code: this.model.value, description: desc }];
        this.items = obj;
        this.selectedItem = obj[0];
    }

    ngOnDestroy() { this.items$.complete(); }
}
