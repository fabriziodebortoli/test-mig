import { URLSearchParams } from '@angular/http';
import { NgForm } from "@angular/forms";
import { animate, transition, trigger, state, style, keyframes, group } from "@angular/animations";
import { EnumsService } from './../../../core/services/enums.service';
import { EventDataService } from './../../../core/services/eventdata.service';
import { DocumentService } from './../../../core/services/document.service';
import { Store } from './../../../core/services/store.service';
import { Component, ViewEncapsulation, ChangeDetectorRef, OnInit, OnDestroy, ElementRef, ViewChild, Input, ChangeDetectionStrategy } from '@angular/core';
import { GridDataResult, PageChangeEvent, SelectionEvent, GridComponent, SelectableSettings } from '@progress/kendo-angular-grid';
import { SortDescriptor, orderBy, CompositeFilterDescriptor } from '@progress/kendo-data-query';
import { Logger } from './../../../core/services/logger.service';
import { LayoutService } from './../../../core/services/layout.service';
import { TbComponentService } from './../../../core/services/tbcomponent.service';
import { DataService } from './../../../core/services/data.service';
import { PaginatorService, ServerNeededParams } from './../../../core/services/paginator.service';
import { FilterService, combineFilters, combineFiltersMap } from './../../../core/services/filter.services';
import { ControlComponent } from './../../../shared/controls/control.component';
import { Subscription, BehaviorSubject, Observable, distinctUntilChanged } from './../../../rxjs.imports';
import { untilDestroy } from './../../commons/untilDestroy';
import { FormMode } from './../../../shared/models/form-mode.enum';
import * as _ from 'lodash';

export const GridStyles = { default: { 'cursor': 'pointer' }, filterTyping: { 'color': 'darkgrey' } };
export const ViewStates = { opened: 'opened', closed: 'closed' };

export class RadarState {
    readonly rows = [];
    readonly columns = []; ager
    readonly selectedIndex: number = 0;
    readonly lastChangedFilterIdx: number = 0;
    readonly view = ViewStates.closed;
    readonly gridStyle = GridStyles.default;
    readonly selectionKeys = [];
}

@Component({
    selector: 'tb-radar',
    templateUrl: './radar.component.html',
    styleUrls: ['./radar.component.scss'],
    encapsulation: ViewEncapsulation.None,
    changeDetection: ChangeDetectionStrategy.OnPush,
    animations: [
        trigger('shrinkOut', [
            state(ViewStates.opened, style({ height: '*' })),
            state(ViewStates.closed, style({ height: 0, overflow: 'hidden' })),
            transition(ViewStates.opened + ' <=> ' + ViewStates.closed, animate('250ms ease-in-out')),
        ])
    ],
    providers: [PaginatorService, FilterService]
})
export class RadarComponent extends ControlComponent implements OnInit, OnDestroy {
    @Input() maxColumns = 10;
    @Input() pageSize = 10;
    @Input() selectionColumnId = 'TBGuid';
    @ViewChild('grid') grid: GridComponent;
    sort$ = new BehaviorSubject<SortDescriptor[]>([]);
    gridData$ = new BehaviorSubject<{ data: any[], total: number, columns: any[] }>({ data: [], total: 0, columns: [] });
    gridStyle$ = new BehaviorSubject<any>(GridStyles.default);
    canNavigate$ = new BehaviorSubject<boolean>(true);
    private _filter: CompositeFilterDescriptor;
    private state = new RadarState();
    pinned = false;
    areFiltersVisible = false;
    public selectableSettings: SelectableSettings;
    private lastSelectedKey: string;
    private lastSelectedKeyPage = -1;

    constructor(public log: Logger, private eventData: EventDataService, private enumsService: EnumsService,
        private elRef: ElementRef, public changeDetectorRef: ChangeDetectorRef, private dataService: DataService,
        private paginator: PaginatorService, private filterer: FilterService, layoutService: LayoutService,
        tbComponentService: TbComponentService, private store: Store) {
        super(layoutService, tbComponentService, changeDetectorRef)
    }

    ngOnInit() {
        this.setSelectableSettings();
        this.filterer.start(400);
        this.paginator.start(1, this.pageSize,
            Observable.combineLatest(this.eventData.showRadar.filter(b => b), this.filterer.filterChanged$, this.sort$,
                (a, b, c) => ({ model: a, customFilters: b, customSort: c })),
            (pageNumber, serverPageSize, otherParams?) => {
                let p = new URLSearchParams();
                p.set('documentID', (this.tbComponentService as DocumentService).mainCmpId);
                p.set('page', (pageNumber + 1).toString());
                p.set('per_page', serverPageSize.toString());
                if (otherParams.customFilters)
                    p.set('customFilters', JSON.stringify(otherParams.customFilters));
                if (otherParams.customSort)
                    p.set('customSort', JSON.stringify(otherParams.customSort));
                return this.dataService.getRadarData(p);
            });
        this.paginator.clientData.pipe(untilDestroy(this)).subscribe(d => {
            this.exitFindMode();
            this.setState(d);
            this.setFocus('[kendofilterinput]', this.state.lastChangedFilterIdx);
        });
        this.filterer.filterChanged$.subscribe(_ => this.gridStyle$.next(GridStyles.default));
        this.filterer.filterChanging$.subscribe(_ => this.gridStyle$.next(GridStyles.filterTyping));
        this.store.select(m => _.get(m, 'FormMode.value'))
            .subscribe(m => this.canNavigate$.next(m !== FormMode.EDIT && m !== FormMode.NEW && m !== FormMode.FIND));
        this.eventData.showRadar.pipe(untilDestroy(this)).subscribe(show => this.show(show));
        super.ngOnInit();
    }

    setState(d: { rows: any[], columns: { caption: string, id: string, type: string }[], total: number }) {
        this.storeViewSelection();
        let maxCols = Math.min(d.columns.length, this.maxColumns);
        const rows = d.columns.length < maxCols ? d.rows :
            d.rows.map(r => [this.selectionColumnId, ...Object.keys(r)]
                .slice(0, maxCols).reduce((o, k) => { o[k] = r[k]; return o; }, {}));
        this.state = {
            ...this.state,
            columns: [d.columns.find(c => c.id === this.selectionColumnId), ...d.columns].slice(0, maxCols),
            rows: rows
        };
        this.gridData$.next({ data: this.state.rows, total: d.total, columns: this.state.columns });
        this.restoreViewSelection();
    }

    private get filter(): CompositeFilterDescriptor {
        return this._filter;
    }

    private set filter(value: CompositeFilterDescriptor) {
        this._filter = _.cloneDeep(value);
        this.filterer.filter = _.cloneDeep(value);
        this.state = { ...this.state, lastChangedFilterIdx: this.state.columns.findIndex(c => c.id === this.filterer.changedField) - 1 };
        this.filterer.onFilterChanged(value);
    }

    filterChange(filter: CompositeFilterDescriptor): void {
        this.filter = filter;
    }

    sortChange(sort: SortDescriptor[]): void {
        this.sort$.next(sort);
    }

    get pinnedIcon() {
        return this.pinned ? 'tb-classicpin' : 'tb-unpin';
    }

    get areFiltersVisibleIcon() {
        return this.areFiltersVisible ? 'tb-filterandsortfilled' : 'tb-filterandsort';
    }

    get pinnedText() {
        return this.pinned ? this._TB('Unpin Radar') : this._TB('Pin Radar');
    }

    get areFiltersVisibleText() {
        return this.areFiltersVisible ? this._TB('Hide Filters') : this._TB('Show Filters');
    }

    async pageChange(event: PageChangeEvent) {
        this.paginator.pageChange(event.skip, event.take);
    }

    selectedKeysChange(e) {
        if (e.length && e[0]) {
            this.state = { ...this.state, selectedIndex: this.state.rows.findIndex(x => x[this.selectionColumnId] === e[0]) };
            this.eventData.radarRecordSelected.emit(e[0]);
        }
    }

    selectByIndex(idx: number) {
        if (this.isFormMode(FormMode.EDIT)) return;
        this.state = { ...this.state, selectedIndex: this.coerce(idx, 0, this.state.rows.length) };
        this.selectByItem(this.state.rows[this.state.selectedIndex]);
    }

    selectByItem(item) {
        if (this.isFormMode(FormMode.EDIT)) return;
        const id = item[this.selectionColumnId];
        this.state = { ...this.state, selectionKeys: [id] };
        this.eventData.radarRecordSelected.emit(id);
    }

    selectAndEdit(item) {
        if (this.isFormMode(FormMode.EDIT)) return;
        if (!this.pinned) this.eventData.showRadar.next(false);
        this.canNavigate$.next(false);
        this.eventData.raiseCommand(this.cmpId, 'ID_EXTDOC_EDIT');
        this.selectByItem(item);
    }

    exitFindMode() {
        if (this.isFormMode(FormMode.FIND))
            this.eventData.raiseCommand(this.cmpId, 'ID_EXTDOC_ESCAPE');
    }

    ngOnDestroy() {
        this.stop();
    }

    private show(show: boolean) {
        this.state = { ...this.state, view: show ? ViewStates.opened : ViewStates.closed };
        this.changeDetectorRef.detectChanges();
    }

    private setFocus(selector: string, index: number) {
        setTimeout(() => {
            const filters = this.elRef.nativeElement.querySelectorAll(selector);
            filters && filters[index] && filters[index].focus();
        }, 100);
    }

    public setSelectableSettings() {
        this.selectableSettings = {
            mode: 'single'
        };
    }

    async selectFirst() {
        this.unselect();
        if (!this.paginator.isFirstPage)
            await this.paginator.firstPage();
        this.selectByIndex(0);
    }

    async selectPrevious() {
        if (this.state.selectedIndex === 0 && this.paginator.currentPage > 0) {
            this.unselect();
            await this.paginator.pageChange((this.paginator.currentPage - 1) * this.pageSize, this.pageSize);
            this.selectByIndex(this.state.rows.length - 1);
            return;
        }
        this.selectByIndex(this.state.selectedIndex - 1);
    }

    async selectNext() {
        if (this.state.selectedIndex === this.state.rows.length - 1) {
            this.unselect();
            await this.paginator.pageChange((this.paginator.currentPage + 1) * this.pageSize, this.pageSize);
            this.selectByIndex(0);
            return;
        }
        this.selectByIndex(this.state.selectedIndex + 1);
    }

    private unselect() {
        this.state = { ...this.state, selectionKeys: [], selectedIndex: -1 };
    }

    get canFirst() {
        return this.state.selectedIndex !== -1 && (!this.paginator.isFirstPage || this.state.selectedIndex > 0);
    }

    get canNext() {
        return this.state.selectedIndex !== -1 && (!this.paginator.noMorePages || this.state.selectedIndex < this.state.rows.length - 1);
    }

    get canPrev() {
        return this.state.selectedIndex !== -1 && (!this.paginator.isFirstPage || this.state.selectedIndex > 0);
    }

    coerce = (value: number, min: number, max: number): number => Math.min(max - 1, Math.max(value, min));
    isFormMode = (formMode: FormMode): boolean => _.get(this.eventData, 'model.FormMode.value') === formMode;
    stop = () => this.paginator.stop();
    toggle = () => this.state = { ...this.state, view: this.state.view === ViewStates.opened ? ViewStates.closed : ViewStates.opened }

    private storeViewSelection() { // workaround for kendo-grid issue 1040
        if (this.state.selectionKeys.length) {
            this.lastSelectedKey = this.state.selectionKeys[0];
        }
    }

    private restoreViewSelection() { // workaround for kendo-grid issue 1040
        this.unselect();
        let idx = this.state.rows.findIndex(x => x[this.selectionColumnId] === this.lastSelectedKey)
        if (idx === -1) return;
        this.state = { ...this.state, selectionKeys: [this.lastSelectedKey], selectedIndex: idx };
    }
}
