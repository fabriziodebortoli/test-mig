import { URLSearchParams } from '@angular/http';
import { NgForm } from "@angular/forms";
import { animate, transition, trigger, state, style, keyframes, group } from "@angular/animations";
import { EnumsService } from './../../../core/services/enums.service';
import { EventDataService } from './../../../core/services/eventdata.service';
import { DocumentService } from './../../../core/services/document.service';
import { Store } from './../../../core/services/store.service';
import { SimpleChanges, Component, ViewEncapsulation, ChangeDetectorRef, OnInit, OnDestroy, ElementRef, ViewChild, Input, ChangeDetectionStrategy, Output, EventEmitter, Directive, ContentChild, TemplateRef } from '@angular/core';
import { GridDataResult, PageChangeEvent, SelectionEvent, GridComponent, SelectableSettings, ColumnReorderEvent } from '@progress/kendo-angular-grid';
import { SortDescriptor, orderBy, CompositeFilterDescriptor } from '@progress/kendo-data-query';
import { Logger } from './../../../core/services/logger.service';
import { LayoutService } from './../../../core/services/layout.service';
import { TbComponentService } from './../../../core/services/tbcomponent.service';
import { DataService } from './../../../core/services/data.service';
import { PaginatorService, ServerNeededParams } from './../../../core/services/paginator.service';
import { ComponentMediator } from './../../../core/services/component-mediator.service';
import { FilterService, combineFilters, combineFiltersMap } from './../../../core/services/filter.services';
import { ControlComponent } from './../../../shared/controls/control.component';
import { Subscription, BehaviorSubject, Observable, distinctUntilChanged } from './../../../rxjs.imports';
import { untilDestroy } from './../../commons/untilDestroy';
import { FormMode } from './../../../shared/models/form-mode.enum';
import * as _ from 'lodash';

export const GridStyles = { default: { 'cursor': 'pointer' }, waiting: { 'color': 'darkgrey' } };

export class State {
    readonly rows = [];
    readonly columns = [];
    readonly selectedIndex: number = 0;
    readonly gridStyle = GridStyles.default;
    readonly selectionKeys = [];
    readonly gridData = { data: [], total: 0, columns: [] };
    readonly canNavigate: boolean = true;
}

@Component({
    selector: 'tb-customisable-grid',
    templateUrl: './customisable-grid.component.html',
    styleUrls: ['./customisable-grid.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush,
    providers: [ComponentMediator]
})
export class CustomisableGridComponent extends ControlComponent implements OnInit, OnDestroy {
    @Input() pageSize = 10;
    @Input() editable = false;
    @Input() canAutoFit = false;
    @Input() selectionColumnId;
    @Input() state: State;
    @Output() selectedKeysChange = new EventEmitter<any>();
    @Output() selectAndEdit = new EventEmitter<any>();
    @Output() selectionChange = new EventEmitter<any>();

    @ContentChild('customisableGridButtonsTemplate', { read: TemplateRef }) customisableGridButtonsTemplate;
    @ViewChild('grid') grid: GridComponent;
    gridStyle$ = new BehaviorSubject<any>(GridStyles.default);
    pinned = false;
    areFiltersVisible = false;
    public selectableSettings: SelectableSettings;
    private lastSelectedKey: string;
    private lastSelectedKeyPage = -1;
    private _filter: CompositeFilterDescriptor;

    constructor(public s: ComponentMediator, private enumsService: EnumsService, private elRef: ElementRef,
        private paginator: PaginatorService, public filterer: FilterService, private store: Store) {
        super(s.layout, s.tbComponent, s.changeDetectorRef);
    }

    ngOnInit() {
        this.setSelectableSettings();
        this.filterer.filterChanged$.subscribe(_ => this.gridStyle$.next(GridStyles.default));
        this.filterer.filterChanging$.subscribe(_ => this.gridStyle$.next(GridStyles.waiting));
        this.paginator.waiting$.subscribe(b =>
            setTimeout(() => this.gridStyle$.next(b ? GridStyles.waiting : GridStyles.default), 0));
        super.ngOnInit();
    }

    ngOnDestroy() {
        super.ngOnDestroy();
        this.stop();
    }

    ngOnChanges(c: SimpleChanges) {
        if (!c.state) return;
        let s = c.state.currentValue as State;
        let cols = this.reorder(s.gridData.columns);
        this.state = { ...this.state, columns: cols, gridData: { ...this.state.gridData, columns: cols } };

    }

    reorder(cols) {
        const sort = JSON.parse(this.s.get('reorderMap'));
        if (sort) cols = cols.sort((a, b) => this.reorderMap[a] - this.reorderMap[b]);
        return cols;
    }

    reorderMap: {};
    columnReorder(e) {
        this.reorderMap[e.column.field] = e.newIndex;
        this.s.set('reorderMap', JSON.stringify(this.reorderMap));
    }

    get areFiltersVisibleIcon() {
        return this.areFiltersVisible ? 'tb-filterandsortfilled' : 'tb-filterandsort';
    }

    get areFiltersVisibleText() {
        return this.areFiltersVisible ? this._TB('Hide Filters') : this._TB('Show Filters');
    }

    private set filter(value: CompositeFilterDescriptor) {
        this._filter = _.cloneDeep(value);
        this.filterer.filter = _.cloneDeep(value);
        this.filterer.lastChangedFilterIdx = this.state.columns
            .findIndex(c => c.id === this.filterer.changedField) - (this.selectionColumnId ? 1 : 0);
        this.filterer.onFilterChanged(value);
    }

    private get filter(): CompositeFilterDescriptor {
        return this._filter;
    }

    filterChange(filter: CompositeFilterDescriptor): void {
        this.filter = filter;
    }

    sortChange(sort: SortDescriptor[]): void {
        this.filterer.sort = sort;
    }

    async pageChange(event: PageChangeEvent) {
        this.paginator.pageChange(event.skip, event.take);
    }

    public setSelectableSettings() {
        this.selectableSettings = {
            mode: 'single'
        };
    }

    stop = () => this.paginator.stop();
}
