import { URLSearchParams } from '@angular/http';
import { NgForm } from "@angular/forms";
import { animate, transition, trigger, state, style, keyframes, group } from "@angular/animations";
import { EnumsService } from './../../../core/services/enums.service';
import { ComponentMediator } from './../../../core/services/component-mediator.service';
import { Store } from './../../../core/services/store.service';
import { Component, ViewEncapsulation, ChangeDetectorRef, OnInit, OnDestroy, ElementRef, ViewChild, Input, ChangeDetectionStrategy, ComponentRef } from '@angular/core';
import { GridDataResult, PageChangeEvent, SelectionEvent, GridComponent, SelectableSettings } from '@progress/kendo-angular-grid';
import { SortDescriptor, orderBy, CompositeFilterDescriptor } from '@progress/kendo-data-query';
import { Logger } from './../../../core/services/logger.service';
import { LayoutService } from './../../../core/services/layout.service';
import { TbComponentService } from './../../../core/services/tbcomponent.service';
import { DataService } from './../../../core/services/data.service';
import { PaginatorService, ServerNeededParams, ClientPage, GridData } from './../../../core/services/paginator.service';
import { FilterService, combineFilters, combineFiltersMap } from './../../../core/services/filter.services';
import { ControlComponent } from './../../../shared/controls/control.component';
import { StorageService } from './../../../core/services/storage.service';
import { Subscription, BehaviorSubject, Observable, distinctUntilChanged, Observer, Subject } from './../../../rxjs.imports';
import { untilDestroy } from './../../commons/untilDestroy';
import { FormMode } from './../../../shared/models/form-mode.enum';
import { State } from './../customisable-grid/customisable-grid.component';
import { ComponentInfoService } from './../../../core/services/component-info.service';
import * as _ from 'lodash';

export const ViewStates = { opened: 'opened', closed: 'closed' };

@Component({
    selector: 'tb-radar',
    templateUrl: './radar.component.html',
    styleUrls: ['./radar.component.scss'],
    animations: [
        trigger('shrinkOut', [
            state(ViewStates.opened, style({ height: '*' })),
            state(ViewStates.closed, style({ height: 0, overflow: 'hidden' })),
            transition(ViewStates.opened + ' <=> ' + ViewStates.closed, animate('250ms ease-in-out')),
        ])
    ],
    changeDetection: ChangeDetectionStrategy.OnPush,
    providers: [PaginatorService, FilterService, ComponentMediator, StorageService]
})
export class RadarComponent extends ControlComponent implements OnInit {
    @Input() maxColumns = 10;
    @Input() pageSize = 10;
    @Input() selectionColumnId = 'TBGuid';
    @ViewChild('grid') grid: GridComponent;
    private set state(s: State) { (this.state$ as BehaviorSubject<State>).next(s); }
    private get state(): State { return (this.state$ as BehaviorSubject<State>).getValue(); }
    state$: Observable<State> = new BehaviorSubject(State.new());
    pinned = false;
    areFiltersVisible = false;
    viewState = ViewStates.closed;
    public selectableSettings: SelectableSettings;
    private lastSelectedKey: string;
    private lastSelectedKeyPage = -1;

    constructor(public m: ComponentMediator, private enumsService: EnumsService,
        private elRef: ElementRef, private store: Store, private cmpInfoService: ComponentInfoService,
        private paginator: PaginatorService, private filterer: FilterService) {
        super(m.layout, m.tbComponent, m.changeDetectorRef)
    }

    ngOnInit() {
        this.filterer.start(400, this.elRef);
        this.paginator.start(1, this.pageSize,
            Observable.combineLatest(this.m.eventData.showRadar.filter(b => b), this.filterer.filterChanged$, this.filterer.sortChanged$,
                (a, b, c) => ({ model: a, customFilters: b, customSort: c })),
            (pageNumber, serverPageSize, data?) => {
                let p = new URLSearchParams();
                p.set('documentID', this.m.document.mainCmpId);
                p.set('page', (pageNumber + 1).toString());
                p.set('per_page', serverPageSize.toString());
                if (data.customFilters) p.set('customFilters', JSON.stringify(data.customFilters));
                if (data.customSort) p.set('customSort', JSON.stringify(data.customSort));
                return this.m.data.getRadarData(p);
            });
        this.paginator.clientData.subscribe(d => {
            this.exitFindMode();
            this.setData(d);
            this.filterer.resetFocus();
        });
        this.store.select(m => _.get(m, 'FormMode.value'))
            .subscribe(m => this.state = this.state
                .with({ canNavigate: m !== FormMode.EDIT && m !== FormMode.NEW && m !== FormMode.FIND }));
        this.m.eventData.showRadar.pipe(untilDestroy(this)).subscribe(show => this.show(show));
        super.ngOnInit();
    }

    setData(d: ClientPage) {
        this.storeViewSelection();
        const data = GridData.new({ data: d.rows, total: d.total, columns: d.columns });
        this.state = this.state.with({ columns: data.columns, rows: data.data, gridData: data });
        this.restoreViewSelection();
    }

    get pinnedIcon() {
        return this.pinned ? 'tb-classicpin' : 'tb-unpin';
    }

    get pinnedText() {
        return this.pinned ? this._TB('Unpin Radar') : this._TB('Pin Radar');
    }

    selectedKeysChange(e) {
        if (e.length && e[0]) {
            this.state = this.state.with({ selectedIndex: this.state.rows.findIndex(x => x[this.selectionColumnId] === e[0]) });
            this.m.eventData.radarRecordSelected.emit(e[0]);
        }
    }

    selectByIndex(idx: number) {
        if (this.isFormMode(FormMode.EDIT)) return;
        this.state = this.state.with({ selectedIndex: this.coerce(idx, 0, this.state.rows.length) });
        this.selectByItem(this.state.rows[this.state.selectedIndex]);
    }

    selectByItem(item) {
        if (this.isFormMode(FormMode.EDIT)) return;
        const id = item[this.selectionColumnId];
        this.state = this.state.with({ selectionKeys: [id] });
        this.m.eventData.radarRecordSelected.emit(id);
    }

    selectAndEdit(item) {
        if (this.isFormMode(FormMode.EDIT)) return;
        if (!this.pinned) this.m.eventData.showRadar.next(false);
        this.state = this.state.with({ canNavigate: false });
        this.selectByItem(item);
        this.m.eventData.raiseCommand(this.cmpId, 'ID_EXTDOC_EDIT');
    }

    exitFindMode() {
        if (this.isFormMode(FormMode.FIND))
            this.m.eventData.raiseCommand(this.cmpId, 'ID_EXTDOC_ESCAPE');
    }

    private show(show: boolean) {
        this.viewState = show ? ViewStates.opened : ViewStates.closed;
        this.changeDetectorRef.detectChanges();
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
        this.state = this.state.with({ selectionKeys: [], selectedIndex: -1 });
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

    private storeViewSelection() {
        if (this.state.selectionKeys.length) {
            this.lastSelectedKey = this.state.selectionKeys[0];
        }
    }

    private restoreViewSelection() {
        this.unselect();
        let idx = this.state.rows.findIndex(x => x[this.selectionColumnId] === this.lastSelectedKey)
        if (idx === -1) return;
        this.state = this.state.with({ ...this.state, selectionKeys: [this.lastSelectedKey], selectedIndex: idx });
    }

    coerce = (value: number, min: number, max: number): number => Math.min(max - 1, Math.max(value, min));
    isFormMode = (formMode: FormMode): boolean => _.get(this.m.eventData, 'model.FormMode.value') === formMode;
    toggle = () => this.viewState = this.viewState === ViewStates.opened ? ViewStates.closed : ViewStates.opened;
}
