import { Component, ChangeDetectorRef, Input } from '@angular/core';
import { URLSearchParams } from '@angular/http';

import { Observable, BehaviorSubject } from '../../../rxjs.imports';

import { TbComponentService } from './../../../core/services/tbcomponent.service';
import { DocumentService } from './../../../core/services/document.service';
import { EnumsService } from './../../../core/services/enums.service';
import { EventDataService } from './../../../core/services/eventdata.service';
import { LayoutService } from './../../../core/services/layout.service';
import { HttpService } from './../../../core/services/http.service';
import { WebSocketService } from './../../../core/services/websocket.service';

import { ControlComponent } from './../control.component';
import { FilterDescriptor, CompositeFilterDescriptor } from '@progress/kendo-data-query';
import { PageChangeEvent } from '@progress/kendo-angular-grid';

import { HlComponent, HotLinkState } from './hotLinkTypes';
import { HotLinkInfo } from './../../models/hotLinkInfo.model';

import { PaginatorService, ServerNeededParams } from '../../../core/services/paginator.service';
import { FilterService, combineFilters, combineFiltersMap, CompositeFilter } from '../../../core/services/filter.service';
import { HyperLinkService, HyperLinkInfo } from '../../../core/services/hyperlink.service';
import { URLSearchParamsBuilder } from './../../commons/builder';

import * as _ from 'lodash';
import { ComponentMediator } from '@taskbuilder/core';

export type HotLinkSlice = { value: any, enabled: boolean, selector: any, type: string, uppercase?: boolean, length?: number }

@Component({
    template: ''
})
export class TbHotLinkBaseComponent extends ControlComponent {

    currentHotLinkNamespace: string;
    defaultPageCounter = 0;
    pageSize = 20;
    info = false;
    type: 'numeric' | 'input' = 'numeric';
    pageSizes = false;
    hotLinkInfo: HotLinkInfo;

    @Input() get modelComponent(): HlComponent { return this._modelComponent; }
    private _modelComponent: HlComponent
    set modelComponent(value: HlComponent) { this._modelComponent = value; this.model = _.get(value, 'model'); }

    private _state = HotLinkState.new();
    state$ = new BehaviorSubject<HotLinkState>(HotLinkState.new());
    set state(state: HotLinkState) { this._state = state; this.state$.next(state); }
    get state(): HotLinkState { return this._state; }

    private _slice$: Observable<HotLinkSlice>;
    set slice$(value: Observable<HotLinkSlice>) { this._slice$ = value; }
    get slice$(): Observable<HotLinkSlice> {
        return (!this.modelComponent || !this.modelComponent.slice$) ? this._slice$ : this.modelComponent.slice$;
    }

    get mainCmpId(): string { return (this.tbComponentService as DocumentService).mainCmpId; }

    afterNoAddOnFly: (oldValue: any, value: any) => void = (oldValue, value) => {
        _.set(this, 'modelComponent.model.value', oldValue);
        this.emitModelChange();
    }
    afterAddOnFly: (value: any) => void = (value) => {
        if (this.hotLinkInfo.mustExistData) {
            _.set(this, 'modelComponent.model.value', value);
            this.emitModelChange();
        }
    }

    protected hyperLinkService: HyperLinkService;

    constructor(layoutService: LayoutService,
        protected webSocketService: WebSocketService,
        protected documentService: DocumentService,
        protected changeDetectorRef: ChangeDetectorRef,
        protected paginatorService: PaginatorService,
        protected filterer: FilterService,
        protected eventDataService: EventDataService,
        protected httpService: HttpService
    ) {
        super(layoutService, documentService, changeDetectorRef);
        this.hyperLinkService = HyperLinkService.New(webSocketService, eventDataService);
    }

    stop() { this.paginatorService.stop(); if(this.filterer) this.filterer.stop(); }

    protected emitModelChange() {
        // setTimeout to avoid ExpressionChangedAfterItHasBeenCheckedError
        setTimeout(() => this.eventDataService.change.emit(this.modelComponent.cmpId));
    }

    readonly getHotLinkNamespace: (srvParams: ServerNeededParams) => string = (srvParams) => {
        if (this.hotLinkInfo.namespace) return this.hotLinkInfo.namespace;
        let hlSelector = _.get(srvParams, 'model.selector');
        if (!hlSelector) throw Error('Namespace Missing in HotLinkInfo')
        return _.get(srvParams, `model.items['${hlSelector}']`);
    }


    readonly createHotLinkSearchParams: (cmpId: string, srvParams: ServerNeededParams, hli: HotLinkInfo, pageNumber: number, serverPageSize: number) => URLSearchParams =
        (cmpId, srvParams, hli, pageNumber, serverPageSize) => URLSearchParamsBuilder.Create(srvParams)
            .withFilter(srvParams.model).withDocumentID(this.mainCmpId).withName(this.hotLinkInfo.name)
            .if()
            .isDefinedInSource('customFilters.logic')
            .isDefinedInSource('customFilters.filters')
            .then()
            .withCustomFilters(srvParams.customFilters)
            .if()
            .isDefinedInSource('customSort')
            .isTrue(_ => srvParams.customSort.length > 0)
            .then()
            .withCustomSort(srvParams.customSort).withDisabled('0').withPage(pageNumber + 1).withRowsPerPage(serverPageSize).build();


    protected get queryTrigger$(): Observable<ServerNeededParams> {
        let filterTrigger$ = this.filterer ? this.filterer.filterChanged$ : Observable.never<CompositeFilter>();
        let sortTrigger$ = this.filterer ? this.filterer.sortChanged$ : Observable.never<any[]>();

        return Observable.combineLatest(this.slice$, filterTrigger$, sortTrigger$,
            (slice, filter, sort) => ({ model: slice.value, customFilters: filter, customSort: sort }));
    }
    readonly queryServer: (page: number, rowsPerPage: number, srvParams: ServerNeededParams) => Observable<any> = (pageNumber, serverPageSize, srvParams) => {
        this.currentHotLinkNamespace = this.getHotLinkNamespace(srvParams);
        let p = this.createHotLinkSearchParams(this.mainCmpId, srvParams, this.hotLinkInfo, pageNumber, serverPageSize);
        return this.httpService.getHotlinkData(this.currentHotLinkNamespace, this.state.selectionType, p);
    }
}