import { URLSearchParams } from '@angular/http';
import { NgForm } from '@angular/forms';
import { animate, transition, trigger, state, style, keyframes, group } from '@angular/animations';
import { EnumsService } from './../../../core/services/enums.service';
import { EventDataService } from './../../../core/services/eventdata.service';
import { DocumentService } from './../../../core/services/document.service';
import { Store } from './../../../core/services/store.service';
import {
    SimpleChanges, Component, ChangeDetectorRef, OnInit, OnDestroy, ElementRef, HostListener,
    ViewChild, Input, ChangeDetectionStrategy, Output, EventEmitter, ContentChild, TemplateRef
} from '@angular/core';
import {
    GridDataResult, PageChangeEvent, SelectionEvent, GridComponent, ColumnReorderEvent,
    PagerSettings
} from '@progress/kendo-angular-grid';
import { SortDescriptor, orderBy, CompositeFilterDescriptor } from '@progress/kendo-data-query';
import { Logger } from './../../../core/services/logger.service';
import { LayoutService } from './../../../core/services/layout.service';
import { TbComponentService } from './../../../core/services/tbcomponent.service';
import { DataService } from './../../../core/services/data.service';
import { PaginatorService, ServerNeededParams, GridData } from './../../../core/services/paginator.service';
import { ComponentMediator } from './../../../core/services/component-mediator.service';
import { FilterService, combineFilters, combineFiltersMap } from './../../../core/services/filter.services';
import { ControlComponent } from './../../../shared/controls/control.component';
import { EnumFilterComponent } from './enum-filter/enum-filter.component';
import { getEnumValueSiblings } from './enum-filter/U';
import { DateFilterComponent } from './date-filter/date-filter.component';
import { Subscription, BehaviorSubject, Observable, distinctUntilChanged } from './../../../rxjs.imports';
import { untilDestroy } from './../../commons/untilDestroy';
import { FormMode } from './../../../shared/models/form-mode.enum';
import { ColumnResizeArgs } from '@progress/kendo-angular-grid';
import { tryOrDefault } from './../../commons/u';
import { get, memoize, cloneDeep } from 'lodash';
import { md5, getObjHash } from './md5';
import { Align } from '@progress/kendo-angular-popup';
import { Collision } from '@progress/kendo-angular-popup';

export const storageKeySuffix = 'custom_grid_settings';
export const GridStyles = { default: { 'cursor': 'pointer' }, waiting: { 'color': 'darkgrey' } };
export class State {
    readonly rows = [];
    readonly columns = [];
    readonly selectedIndex: number = 0;
    readonly gridStyle = GridStyles.default;
    readonly selectionKeys = [];
    readonly gridData = GridData.new();
    readonly canNavigate: boolean = true;

    static new(a?: Partial<State>): Readonly<State> {
        if (a) return new State().with(a);
        return new State();
    }
    with(a: (Partial<State> | ((s: State) => Partial<State>))): Readonly<State> {
        if (typeof a === 'function')
            a = a(this as any);
        return Object.assign(new State(), this, a) as any;
    }
}
// export class State extends Record(_State) { }

export class Settings {
    version = 2;
    reorderMap: string[] = [];
    shownColumns: { [name: string]: boolean } = {};
    widths: { [name: string]: number } = {};
    hash: string;
}

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
    @Input() resizable = true;
    @Input() popupAnchor: ElementRef;
    @Input() anchorAlign: Align = { horizontal: 'right', vertical: 'top' };
    @Input() popupAlign: Align = { horizontal: 'right', vertical: 'bottom' };
    @Input() pageable = true;
    @Input()
    get state(): State { return this._state; }
    set state(value: State) {
        let gd = this.reshape(value.gridData, value.columns);
        this._state = value.with({ columns: gd.columns, rows: gd.data, gridData: gd });
    }
    @Output() selectedKeysChange = new EventEmitter<any>();
    @Output() selectAndEdit = new EventEmitter<any>();
    @Output() selectionChange = new EventEmitter<any>();
    @ContentChild('customisableGridButtonsTemplate', { read: TemplateRef }) customisableGridButtonsTemplate;
    @ViewChild('grid') grid: GridComponent;
    @ViewChild('anchor') public anchor: ElementRef;
    @ViewChild('popup', { read: ElementRef }) public popup: ElementRef;
    gridStyle$ = new BehaviorSubject<any>(GridStyles.default);
    pinned = false;
    areFiltersVisible = false;
    showColumnHandler = false;
    collision: Collision = { horizontal: 'flip', vertical: 'fit' };
    private lastSelectedKey: string;
    private lastSelectedKeyPage = -1;
    private _filter: CompositeFilterDescriptor;
    private _state: State;
    private _settings = new Settings();
    private reshape = memoize((d: GridData, cols) =>
        tryOrDefault(() => this.limit(d).with(s => ({ columns: this.reorder(this.resize(s.columns)) })), d));

    constructor(public m: ComponentMediator, private enumsService: EnumsService, private elRef: ElementRef,
        public paginator: PaginatorService, public filterer: FilterService, private store: Store) {
        super(m.layout, m.tbComponent, m.changeDetectorRef);
    }

    ngOnInit() {
        this.loadSettings();
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

    loadSettings = () => this._settings = this.m.storage.getOrDefault(storageKeySuffix, new Settings());

    saveSettings = () => this.m.storage.set(storageKeySuffix, this._settings);

    saveWidths = () => {
        this._settings.widths = this.grid.columns.reduce((o, c) => ({ ...o, [c['field']]: c.width }), {});
        this.saveSettings();
    }

    limit = (d: GridData): GridData => {
        if (d.columns.length === 0) return d;
        const maxCols = Math.min(d.columns.length, this.maxColumns);
        const data = d.columns.length < maxCols ? d.data :
            d.data.map(r => this.moveToStart(c => this.selectionColumnId && c === this.selectionColumnId, Object.keys(r))
                .slice(0, maxCols).reduce((o, k) => ({ ...o, [k]: r[k] }), {}));
        const columns = this.moveToStart(c => this.selectionColumnId && c.id === this.selectionColumnId, d.columns)
            .slice(0, maxCols);
        this.resetSettingsIfNew(columns);
        return d.with({ data, columns });
    }

    moveToStart = <T>(predicate: (value: T) => boolean, array: T[]): T[] =>
        [array.find(predicate), ...array.filter(v => !predicate(v))].filter(x => x);

    resize = cols => {
        if (!this.resizable || cols.length === 0) return cols;
        const widths = this._settings.widths;
        return cols.map(c => ({ ...c, width: widths[c.id] || 180 }));
    }

    reorder = cols => {
        if (cols.length === 0) return cols;
        const reorderMap = this._settings.reorderMap;
        return cols.sort((a, b) => reorderMap.indexOf(a.id) - reorderMap.indexOf(b.id));
    };

    /** columns returned by server could change over time... */
    private resetSettingsIfNew = cols => {
        if (!this._settings) this.loadSettings();
        if (cols.length === 0) return this._settings;
        let hash = getObjHash(cols, this.maxColumns);
        if (new Settings().version !== this._settings.version || hash !== this._settings.hash || !this._settings.shownColumns.length) {
            this._settings = new Settings();
            this._settings.hash = hash;
            this._settings.reorderMap = cols.map(x => x.id);
            this._settings.shownColumns = cols.reduce((o, c) => ({ ...o, [c.id]: true }), {});
            this.saveSettings();
        }
        return this._settings;
    }

    columnReorder(e) {
        setTimeout(() => {
            this._settings.reorderMap = this.grid.columns.toArray().sort((a, b) => a.orderIndex - b.orderIndex).map(c => c['field']);
            this.saveSettings();
        }, 0);
    }

    columnResize(es: any[]) {
        if (!this.resizable) return;
        es.forEach(e => this._settings.widths[e.column.field] = e.newWidth);
        this.saveSettings();
    }

    autofit() {
        this.grid.autoFitColumns()
        this.saveWidths();
    }

    get areFiltersVisibleIcon() {
        return this.areFiltersVisible ? 'tb-filterandsortfilled' : 'tb-filterandsort';
    }

    get areFiltersVisibleText() {
        return this.areFiltersVisible ? this._TB('Hide Filters') : this._TB('Show Filters');
    }

    public set filter(value: CompositeFilterDescriptor) {
        this._filter = cloneDeep(value);
        this.filterer.filter = cloneDeep(value);
        this.filterer.onFilterChanged(value);
    }

    public get filter(): CompositeFilterDescriptor { return this._filter; }

    filterChange(filter: CompositeFilterDescriptor): void { this.filter = filter; }

    sortChange(sort: SortDescriptor[]): void { this.filterer.sort = sort; }

    async pageChange(event: PageChangeEvent) { this.paginator.pageChange(event.skip, event.take); }

    stop = () => this.paginator.stop();

    get settings() { return this._settings; }

    @HostListener('document:click', ['$event'])
    public documentClick(event: any): void {
        if (this.showColumnHandler && !this.contains(event.target))
            this.showColumnHandler = false;
    }

    private contains(target: any): boolean {
        return (this.popupAnchor || this.anchor).nativeElement.contains(target) ||
            (this.popup ? this.popup.nativeElement.contains(target) : false);
    }

    tbTypeToFilter(type: string) {
        const map = {
            'Boolean': 'boolean',
            'DateTime': 'date',
            'String': 'text',
            'Enum': 'text',
            'Int64': 'text'
        };
        return map[type] || 'text';
    }

    // Object.keys(localStorage).filter(k => k.startsWith("storage")).reduce((o, v) => ({...o, [v]:JSON.parse(localStorage[v])}), {})
}
