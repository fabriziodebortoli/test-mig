import { URLSearchParams } from '@angular/http';
import { NgForm } from "@angular/forms";
import { animate, transition, trigger, state, style, keyframes, group } from "@angular/animations";
import { EnumsService } from './../../../core/services/enums.service';
import { EventDataService } from './../../../core/services/eventdata.service';
import { DocumentService } from './../../../core/services/document.service';
import { Store } from './../../../core/services/store.service';
import { Component, ViewEncapsulation, ChangeDetectorRef, OnDestroy, ElementRef, ViewChild, Input, ChangeDetectionStrategy } from '@angular/core';
import { GridDataResult, PageChangeEvent, SelectionEvent, GridComponent } from '@progress/kendo-angular-grid';
import { SortDescriptor, orderBy, CompositeFilterDescriptor } from '@progress/kendo-data-query';
import { Logger } from './../../../core/services/logger.service';
import { LayoutService } from './../../../core/services/layout.service';
import { TbComponentService } from './../../../core/services/tbcomponent.service';
import { DataService } from './../../../core/services/data.service';
import { PaginatorService, ServerNeededParams } from './../../../core/services/paginator.service';
import { FilterService, combineFilters, combineFiltersMap } from './../../../core/services/filter.services';
import { ControlComponent } from './../../../shared/controls/control.component';
import { Subscription, BehaviorSubject, Observable } from './../../../rxjs.imports';
import { untilDestroy } from './../../commons/untilDestroy';
import { FormMode } from './../../../shared/models/form-mode.enum';
import * as _ from 'lodash';

export const GridStyles = { default: { 'cursor': 'pointer' }, filterTyping: { 'color': 'darkgrey' } };
export const ViewStates = { opened: 'opened', closed: 'closed' };

export class RadarState {
    readonly rows = [];
    readonly columns = [];
    readonly selectedIndex: number = 0;
    readonly lastChangedFilterIndex: number = 0;
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
            transition('opened <=> closed', animate('250ms ease-in-out')),
        ])
    ],
    providers: [PaginatorService, FilterService]
})
export class RadarComponent extends ControlComponent implements OnDestroy {
    @Input() pageSize: number = 7;
    @Input() selectionColumnId: string = 'TBGuid';
    @ViewChild('grid') grid: GridComponent;
    sort: SortDescriptor[] = [];
    gridData$ = new BehaviorSubject<{ data: any[], total: number, columns: any[] }>({ data: [], total: 0, columns: [] });
    gridStyle$ = new BehaviorSubject<any>(GridStyles.default);
    private _filter: CompositeFilterDescriptor;
    private state = new RadarState();
    pinned = false;
    canNavigate: Observable<boolean>;

    constructor(public log: Logger, private eventData: EventDataService, private enumsService: EnumsService,
        private elRef: ElementRef, public changeDetectorRef: ChangeDetectorRef, private dataService: DataService,
        private paginator: PaginatorService, private filterer: FilterService, layoutService: LayoutService,
        tbComponentService: TbComponentService, private store: Store) {
        super(layoutService, tbComponentService, changeDetectorRef)
        this.start();
    }

    private start() {
        this.filterer.configure(200);
        this.paginator.start(1, this.pageSize,
            combineFiltersMap(this.eventData.showRadar.filter(b => b), this.filterer.filterChanged$, (l, r) => ({ customFilters: l, model: r })),
            (pageNumber, serverPageSize, otherParams?) => {
                let p = new URLSearchParams();
                p.set('documentID', (this.tbComponentService as DocumentService).mainCmpId);
                p.set('filter', JSON.stringify(otherParams.model.value));
                p.set('customFilters', JSON.stringify(otherParams.customFilters));
                p.set('page', JSON.stringify(pageNumber + 1)); // test numbers
                p.set('per_page', JSON.stringify(serverPageSize));
                return this.dataService.getRadarData(p);
            });
        this.paginator.clientData.pipe(untilDestroy(this)).subscribe(d => {
            this.setState(d);
            this.setFocus('[kendofilterinput]', this.state.lastChangedFilterIndex);
        });
        this.filterer.filterChanged$.subscribe(x => {
            this.gridData$.next({ data: [], total: 0, columns: this.state.columns });
            this.gridStyle$.next(GridStyles.default);
        });
        this.filterer.filterChanging$.subscribe(x => this.gridStyle$.next(GridStyles.filterTyping));
        this.canNavigate = this.store.select(m => m.FormMode.value).map(m => m !== FormMode.EDIT);
        this.eventData.showRadar.pipe(untilDestroy(this)).subscribe(show => this.show(show));
    }

    setState(d: { rows: any[], columns: { caption: string, id: string, type: string }[], total: number }, maxColumns = 10) {
        maxColumns = Math.min(d.columns.length, maxColumns);
        const rows = d.columns.length < maxColumns ? d.rows :
            d.rows.map(r => [this.selectionColumnId, ...Object.keys(r)].slice(0, maxColumns).reduce((o, k) => { o[k] = r[k]; return o; }, {}));
        this.state = {
            ...this.state,
            columns: [d.columns.find(c => c.id == this.selectionColumnId), ...d.columns].slice(0, maxColumns),
            rows: rows
        };
        this.gridData$.next({ data: this.state.rows, total: d.total, columns: this.state.columns });
    }

    private get filter(): CompositeFilterDescriptor {
        return this._filter;
    }

    private set filter(value: CompositeFilterDescriptor) {
        this._filter = _.cloneDeep(value);
        this.filterer.filter = _.cloneDeep(value);
        this.state = { ...this.state, lastChangedFilterIndex: this.state.columns.findIndex(c => c.id === this.filterer.changedField) };
        this.filterer.onFilterChanged(value);
    }

    async pageChange(event: PageChangeEvent) {
        await this.paginator.pageChange(event.skip, event.take);
    }

    sortChange(sort: SortDescriptor[]): void {
        this.sort = sort;
        // this.setState();
    }

    selectionChanged(event: SelectionEvent) {
        if (!event.selected) return;
        this.selectByIndex(event.index);
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
        this.selectByItem(item);
        this.eventData.raiseCommand(this.cmpId, 'ID_EXTDOC_EDIT');
    }

    exitFindMode() {
        if (this.isFormMode(FormMode.EDIT))
            this.eventData.raiseCommand(this.cmpId, 'ID_EXTDOC_ESCAPE');
    }

    ngOnDestroy() {
        this.stop();
    }

    private show(show: boolean) {
        show && this.exitFindMode();
        this.state = { ...this.state, view: show ? ViewStates.opened : ViewStates.closed };
    }

    private setFocus(selector: string, index: number) {
        setTimeout(() => {
            const filters = this.elRef.nativeElement.querySelectorAll(selector);
            filters && filters[index] && filters[index].focus();
        }, 100);
    }

    isFormMode = (formMode: FormMode): boolean => _.get(this.eventData, 'model.FormMode.value') === formMode;
    coerce = (value: number, min: number = 0, max: number): number => Math.min(max - 1, Math.max(value, min));
    selectPrevious = () => this.selectByIndex(this.state.selectedIndex - 1);
    selectNext = () => this.selectByIndex(this.state.selectedIndex + 1);
    stop = () => this.paginator.stop();
    toggle = () => this.state = { ...this.state, view: this.state.view === ViewStates.opened ? ViewStates.closed : ViewStates.opened };
}
