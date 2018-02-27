import { TbComponentService } from './../../../core/services/tbcomponent.service';
import { DocumentService } from './../../../core/services/document.service';
import { EnumsService } from './../../../core/services/enums.service';
import { EventDataService } from './../../../core/services/eventdata.service';
import { LayoutService } from './../../../core/services/layout.service';
import { StorageService } from './../../../core/services/storage.service';
import { TbHotLinkBaseComponent } from './../hot-link-base/tb-hot-link-base.component';
import { HttpService } from './../../../core/services/http.service';
import { ComponentMediator } from './../../../core/services/component-mediator.service';
import { OnDestroy, OnInit, Component, Input, ViewContainerRef, ChangeDetectorRef, AfterViewInit } from '@angular/core';
import { URLSearchParams } from '@angular/http';
import { PageChangeEvent } from '@progress/kendo-angular-grid';
import { FilterDescriptor, CompositeFilterDescriptor } from '@progress/kendo-data-query';
import { PopupService, PopupSettings, PopupRef } from '@progress/kendo-angular-popup';
import { BehaviorSubject, Subscription, Observable } from '../../../rxjs.imports';
import { PaginatorService, ServerNeededParams, GridData } from '../../../core/services/paginator.service';
import { FilterService, combineFilters, combineFiltersMap } from '../../../core/services/filter.services';
import { HyperLinkService, HyperLinkInfo } from '../../../core/services/hyperlink.service';
import { HotLinkInfo } from './../../models/hotLinkInfo.model';
import { untilDestroy } from './../../commons/untilDestroy';
import { HlComponent, HotLinkState } from './../hot-link-base/hotLinkTypes';
import * as _ from 'lodash';

declare var document: any;

@Component({
  selector: 'tb-hotlink-combo',
  templateUrl: './tb-hot-link-combo.component.html',
  styleUrls: ['./tb-hot-link-combo.component.scss'],
  providers: [PaginatorService, FilterService, HyperLinkService, ComponentMediator, StorageService]
})
export class TbHotlinkComboComponent extends TbHotLinkBaseComponent implements OnDestroy, OnInit, AfterViewInit {

  constructor(layoutService: LayoutService,
    protected httpService: HttpService,
    protected documentService: DocumentService,
    protected changeDetectorRef: ChangeDetectorRef,
    protected eventDataService: EventDataService,
    protected paginator: PaginatorService,
    protected filterer: FilterService, 
    protected hyperLinkService: HyperLinkService,
    protected vcr: ViewContainerRef,
    protected mediator: ComponentMediator
  ) {
    super(layoutService, documentService, changeDetectorRef, paginator, filterer, hyperLinkService, eventDataService);
  }

  private dropDownOpened = false;
  openDropDown() {
    this.start();
    this.dropDownOpened = true;
  }

  closeDropDown() {
    this.stop();
    this.dropDownOpened = false;
    this.disablePager = true;
  }

  disablePager: boolean = true;
  get enablePager(): boolean {
    return !this.disablePager;
  }

  protected start() {
    this.defaultPageCounter = 0;
    this.filterer.start(200);
    this.paginator.start(1, this.pageSize,
      Observable
        .combineLatest(this.slice$, this.filterer.filterChanged$, this.filterer.sortChanged$,
          (slice, filter, sort) => ({ model: slice, customFilters: filter, customSort: sort })),
      (pageNumber, serverPageSize, args) => {
        let ns = this.hotLinkInfo.namespace;
        if (!ns && args.model.selector && args.model.selector !== '') {
          ns = args.model.items[args.model.selector].namespace;
        }
        this.currentHotLinkNamespace = ns;
        let p: URLSearchParams = new URLSearchParams();
        p.set('filter', JSON.stringify(args.model.value));
        p.set('documentID', (this.tbComponentService as DocumentService).mainCmpId);
        p.set('hklName', this.hotLinkInfo.name);
        if (args.customFilters && args.customFilters.logic && args.customFilters.filters && args.customFilters.field)
          p.set('customFilters', JSON.stringify(args.customFilters));
        if (args.customSort)
          p.set('customSort', JSON.stringify(args.customSort));
        p.set('disabled', '0');
        p.set('page', JSON.stringify(pageNumber + 1));
        p.set('per_page', JSON.stringify(serverPageSize));
        return this.httpService.getHotlinkData(ns, this.state.selectionType, p);
      });

    // this.state = this.state.with();

    this.paginator.clientData.subscribe((d) => {
      this.state = this.state.with({ selectionColumn: d.key, gridData: GridData.new({ data: d.rows, total: d.total, columns: d.columns })});
    });

    this.filterer.filterChanged$.filter(x => x.logic !== undefined)
      .subscribe(x => {
        if (this.modelComponent && this.modelComponent.model) {
          this.modelComponent.model.value = _.get(x, 'filters[0].value');
          this.emitModelChange();
        }
      });

    this.paginator.waiting$.pipe(untilDestroy(this))
      .subscribe(waiting => {
        this.disablePager = (this.paginator.isFirstPage && this.paginator.noMorePages) || (waiting && this.paginator.isJustInitialized);
      });
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

  public onFilterChange(filter: string): void {
    if (this.dropDownOpened) {
      if (filter === '' || !filter) this.filter = { logic: 'and', filters: [] };
      else this.filter = { logic: 'and', filters: [{ field: this.state.selectionColumn, operator: 'contains', value: filter }] };
    }
  }

  selectionChanged(value: any) {
    _.set(this.eventDataService.model, this.hotLinkInfo.name + '.Description.value', _.get(value, 'displayString'));
    if (this.modelComponent && this.modelComponent.model) {
      this.modelComponent.model.value = _.get(value, 'id');
      this.emitModelChange();
    }
  }

  private _hyperLinkElement: HTMLElement;
  private get hyperLinkElement(): HTMLElement {
    if(this._hyperLinkElement) return this._hyperLinkElement;
    let searchBar = (this.vcr.element.nativeElement.parentNode.getElementsByClassName('k-searchbar') as HTMLCollection).item(0) as HTMLElement;
    if(searchBar) {
      this._hyperLinkElement = (searchBar.getElementsByClassName('k-input') as HTMLCollection).item(0) as HTMLElement;
      return this._hyperLinkElement;
    }
    return undefined; 
  }

  ngAfterViewInit(): void {
    this.hyperLinkService.start(this.hyperLinkElement, 
      { name: this.hotLinkInfo.name,
        cmpId: this.documentService.mainCmpId, 
        enableAddOnFly: this.hotLinkInfo.enableAddOnFly, 
        mustExistData: this.hotLinkInfo.mustExistData,
        model: this.modelComponent.model }, 
        this.slice$, this.clearModel, this.afterAddOnFly);
  }

  ngOnInit() {
    this.mediator.storage.options.componentInfo.cmpId = this.modelComponent.cmpId;
    this.state = this.state.with({selectionType: 'combo'});
  }

  ngOnDestroy() {
  }
}