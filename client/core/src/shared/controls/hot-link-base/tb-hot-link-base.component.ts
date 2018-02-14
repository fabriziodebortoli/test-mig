import { Component, ChangeDetectorRef, Input } from '@angular/core';

import { Observable, BehaviorSubject } from '../../../rxjs.imports';

import { TbComponentService } from './../../../core/services/tbcomponent.service';
import { DocumentService } from './../../../core/services/document.service';
import { EnumsService } from './../../../core/services/enums.service';
import { EventDataService } from './../../../core/services/eventdata.service';
import { LayoutService } from './../../../core/services/layout.service';
import { ControlComponent } from './../control.component';
import { FilterDescriptor, CompositeFilterDescriptor } from '@progress/kendo-data-query';
import { PageChangeEvent  } from '@progress/kendo-angular-grid';

import { HlComponent, HotLinkState } from './hotLinkTypes';
import { HotLinkInfo } from './../../models/hotLinkInfo.model';

import { PaginatorService, ServerNeededParams } from '../../../core/services/paginator.service';
import { FilterService, combineFilters, combineFiltersMap } from '../../../core/services/filter.services';
import { HyperLinkService, HyperLinkInfo} from '../../../core/services/hyperlink.service';

import * as _ from 'lodash';

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

    private _modelComponent: HlComponent
    @Input() public get modelComponent(): HlComponent { return this._modelComponent; }

    public set modelComponent(value: HlComponent) {
        this._modelComponent = value;
        if (value && value.model) { this.model = value.model; }
    }

    public hotLinkInfo: HotLinkInfo;

    private _slice$: Observable<{ value: any, enabled: boolean, selector: any }>;
    public set slice$(value: Observable<{ value: any, enabled: boolean, selector: any }>) {
        this._slice$ = value;
    }
    public get slice$(): Observable<{ value: any, enabled: boolean, selector: any }>{
        return (!this.modelComponent || !this.modelComponent.slice$) ?  this._slice$ : this.modelComponent.slice$;
    }

    private _state = new HotLinkState();
    public state$ = new BehaviorSubject(this._state);
    public set state(state: HotLinkState) {
        this._state = state;
        this.state$.next(state);
    }
    public get state(): HotLinkState {
        return this._state;
    }

    private _filter: CompositeFilterDescriptor;
    public get filter(): CompositeFilterDescriptor {
        return this._filter;
    }

    public set filter(value: CompositeFilterDescriptor) {
        this._filter = _.cloneDeep(value);
        this.filterer.filter = _.cloneDeep(value);
        this.filterer.onFilterChanged(value);
    }
    
    constructor(layoutService: LayoutService,
                protected documentService: DocumentService,
                protected changeDetectorRef: ChangeDetectorRef,
                protected paginator: PaginatorService,
                protected filterer: FilterService,
                protected hyperLinkService: HyperLinkService,
                protected eventDataService: EventDataService
                ) {
        super(layoutService, documentService, changeDetectorRef);
    }

    protected start() { };

    protected stop() {
        this.paginator.stop();
        this.filterer.stop();
    }

    protected emitModelChange() {
        // setTimeout to avoid ExpressionChangedAfterItHasBeenCheckedError
        setTimeout(() => this.eventDataService.change.emit(this.modelComponent.cmpId));
      }
    
    public onFilterChange(filter: CompositeFilterDescriptor): void {
        this.filter = filter;
    }
    
    protected async pageChange(event: PageChangeEvent) {
        await this.paginator.pageChange(event.skip, event.take);
    }
    
    protected async nextDefaultPage() {
        this.defaultPageCounter++;
        await this.paginator.pageChange(this.defaultPageCounter * this.pageSize, this.pageSize);
    }
    
    protected async prevDefaultPage() {
        this.defaultPageCounter--;
        await this.paginator.pageChange(this.defaultPageCounter * this.pageSize, this.pageSize);
    }
    
    protected async firstDefaultPage() {
        this.defaultPageCounter = 0;
        await this.paginator.pageChange(0, this.pageSize);
    }
}