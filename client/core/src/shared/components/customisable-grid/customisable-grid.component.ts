import { URLSearchParams } from '@angular/http';
import { NgForm } from '@angular/forms';
import { animate, transition, trigger, state, style, keyframes, group } from '@angular/animations';
import { EnumsService } from './../../../core/services/enums.service';
import { EventDataService } from './../../../core/services/eventdata.service';
import { DocumentService } from './../../../core/services/document.service';
import { Store } from './../../../core/services/store.service';
import {
    SimpleChanges, Component, ChangeDetectorRef, OnInit, OnDestroy, ElementRef,
    ViewChild, Input, ChangeDetectionStrategy, Output, EventEmitter, ContentChild, TemplateRef
} from '@angular/core';
import { GridDataResult, PageChangeEvent, SelectionEvent, GridComponent, ColumnReorderEvent } from '@progress/kendo-angular-grid';
import { SortDescriptor, orderBy, CompositeFilterDescriptor } from '@progress/kendo-data-query';
import { Logger } from './../../../core/services/logger.service';
import { LayoutService } from './../../../core/services/layout.service';
import { TbComponentService } from './../../../core/services/tbcomponent.service';
import { DataService } from './../../../core/services/data.service';
import { PaginatorService, ServerNeededParams, GridData } from './../../../core/services/paginator.service';
import { ComponentMediator } from './../../../core/services/component-mediator.service';
import { FilterService, combineFilters, combineFiltersMap } from './../../../core/services/filter.services';
import { ControlComponent } from './../../../shared/controls/control.component';
import { Subscription, BehaviorSubject, Observable, distinctUntilChanged } from './../../../rxjs.imports';
import { untilDestroy } from './../../commons/untilDestroy';
import { FormMode } from './../../../shared/models/form-mode.enum';
import { Record } from './../../../shared/commons/mixins/record';
import * as _ from 'lodash';
import { ColumnResizeArgs } from '@progress/kendo-angular-grid';

export const GridStyles = { default: { 'cursor': 'pointer' }, waiting: { 'color': 'darkgrey' } };

export class State extends Record(class {
    readonly rows = [];
    readonly columns = [];
    readonly selectedIndex: number = 0;
    readonly gridStyle = GridStyles.default;
    readonly selectionKeys = [];
    readonly gridData = new GridData();
    readonly canNavigate: boolean = true;
}) { }

export class Settings {
    version = 1;
    reorderMap: number[] = [];
    hiddenColumns: string[] = [];
    widths: { [name: string]: number } = {};
}

export const storageKeySuffix = 'custom_grid_settings';

@Component({
    selector: 'tb-customisable-grid',
    templateUrl: './customisable-grid.component.html',
    styleUrls: ['./customisable-grid.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class CustomisableGridComponent extends ControlComponent implements OnInit, OnDestroy {
    @Input() pageSize = 10;
    @Input() editable = false;
    @Input() canAutoFit = false;
    @Input() maxColumns = 10;
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
    private lastSelectedKey: string;
    private lastSelectedKeyPage = -1;
    private _filter: CompositeFilterDescriptor;
    private _state: State;

    _currentData: GridData;
    get data(): GridData {
        if (this.state.gridData === this._currentData) return this._currentData;
        this._currentData = this.reshape(this.state.gridData);
        this.state = this.state.with(s => ({ columns: this._currentData.columns, gridData: this._currentData }));
        return this.state.gridData;
    }

    constructor(public s: ComponentMediator, private enumsService: EnumsService, private elRef: ElementRef,
        private paginator: PaginatorService, public filterer: FilterService, private store: Store) {
        super(s.layout, s.tbComponent, s.changeDetectorRef);
    }

    ngOnInit() {
        this.filterer.filterChanged$.subscribe(_ => this.gridStyle$.next(GridStyles.default));
        this.filterer.filterChanging$.subscribe(_ => this.gridStyle$.next(GridStyles.waiting));
        this.paginator.waiting$.subscribe(b =>
            setTimeout(() => this.gridStyle$.next(b ? GridStyles.waiting : GridStyles.default), 0));
        super.ngOnInit();
    }

    ngOnDestroy() {
        this.saveWidths();
        super.ngOnDestroy();
        this.stop();
    }

    saveWidths = () =>
        this.s.storage.using(storageKeySuffix, new Settings(), s =>
            ({ ...s, widths: this.grid.columns.reduce((o, c) => ({ ...o, [c['field']]: c.width }), {}) })
        );


    reshape = (d: GridData) => this.limit(d).with(s => ({ columns: this.reorder(this.resize(s.columns)) }));

    limit = (d: GridData): GridData => {
        const maxCols = Math.min(d.columns.length, this.maxColumns);
        const data = d.columns.length < maxCols ? d.data :
            d.data.map(r => [this.selectionColumnId, ...Object.keys(r)]
                .slice(0, maxCols).reduce((o, k) => ({ ...o, [k]: r[k] }), {}));
        const columns = [d.columns.find(c => c.id === this.selectionColumnId), ...d.columns]
            .slice(0, maxCols);
        return d.with({ data, columns });
    }

    resize = cols => {
        if (!cols || cols.length === 0) return cols;
        const widths = this.getSettingsFor(cols).widths;
        return cols.map(c => ({ ...c, width: widths[c.id] || 180 }));
    }

    reorder = cols => {
        if (!cols || cols.length === 0) return cols;
        const reorderMap = this.getSettingsFor(cols).reorderMap;
        const weight = reorderMap.reduce((o, v, i) => ({ ...o, [v]: i }), {});
        return cols.sort((a, b) => weight[a.id] - weight[b.id]);
    };

    private getSettingsFor = cols =>
        this.s.storage.using(storageKeySuffix, new Settings(), s => {
            if (!s.reorderMap || cols.length !== 0 && Object.keys(s.reorderMap).length !== cols.length) // todo pd: da rivedere, in caso ti cambiamenti alla query, magari con un hash md5 o sda1
                return { ...s, reorderMap: cols.map(x => x.id) };
            return s;
        });

    columnReorder(e) {
        const s = this.getSettingsFor(this.state.columns);
        const d = this.selectionColumnId ? 1 : 0;
        const elem = s.reorderMap[e.oldIndex + d];
        s.reorderMap.splice(e.oldIndex + d, 1);
        s.reorderMap.splice(e.newIndex + d, 0, elem);
        this.s.storage.set<Settings>(storageKeySuffix, s);
    }

    columnResize(es: any[]) {
        const settings = this.s.storage.getOrDefault<Settings>(storageKeySuffix, new Settings());
        this.s.storage.using(storageKeySuffix, new Settings(), s => {
            es.forEach(e => s.widths[e.column.field] = e.newWidth);
            return s;
        });
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

    stop = () => this.paginator.stop();

    public get settings() {
        return this.s.storage.getOrDefault<Settings>(storageKeySuffix, new Settings());
    }

    public restoreColumns(): void {
        this.s.storage.using(storageKeySuffix, new Settings(), s => ({ ...s, hiddenColumns: [] }));
    }

    public hideColumn(field: string): void {
        this.s.storage.using(storageKeySuffix, new Settings(), s =>
            ({ ...s, hiddenColumns: [...s.hiddenColumns, field] }));
    }
}
