import { TbComponentService } from './../../../core/services/tbcomponent.service';
import { DocumentService } from './../../../core/services/document.service';
import { EnumsService } from './../../../core/services/enums.service';
import { EventDataService } from './../../../core/services/eventdata.service';
import { LayoutService } from './../../../core/services/layout.service';
import { StorageService } from './../../../core/services/storage.service';
import { TbHotLinkBaseComponent } from './../hot-link-base/tb-hot-link-base.component';
import { HttpService } from './../../../core/services/http.service';
import { ComponentMediator } from './../../../core/services/component-mediator.service';
import { OnDestroy, OnInit, Component, Input, ViewContainerRef, ChangeDetectorRef, ViewChild, AfterViewInit } from '@angular/core';
import { URLSearchParams } from '@angular/http';
import { PageChangeEvent } from '@progress/kendo-angular-grid';
import { FilterDescriptor, CompositeFilterDescriptor } from '@progress/kendo-data-query';
import { PopupService, PopupSettings, PopupRef } from '@progress/kendo-angular-popup';
import { Subject, BehaviorSubject, Subscription, Observable, debounceFirst } from '../../../rxjs.imports';
import { PaginatorService, ServerNeededParams, GridData } from '../../../core/services/paginator.service';
import { FilterService, combineFilters, combineFiltersMap } from '../../../core/services/filter.service';
import { HyperLinkService, HyperLinkInfo } from '../../../core/services/hyperlink.service';
import { HotLinkInfo } from './../../models/hotLinkInfo.model';
import { untilDestroy } from './../../commons/untilDestroy';
import { HlComponent, HotLinkState } from './../hot-link-base/hotLinkTypes';
import { HotLinkComboSelectionType, DescriptionHotLinkSelectionType } from './../hot-link-base/hotLinkTypes';
import { TbHotlinkComboHyperLinkHandler } from './hyper-link-handler';
import { TbHotlinkComboEventHandler } from './event-handler';

import * as _ from 'lodash';
import { ComboBoxData } from '../../models/combo-box-data';

declare var document: any;

@Component({
  selector: 'tb-hotlink-combo',
  templateUrl: './tb-hot-link-combo.component.html',
  styleUrls: ['./tb-hot-link-combo.component.scss'],
  providers: [PaginatorService, FilterService, HyperLinkService, ComponentMediator, StorageService]
})
export class TbHotlinkComboComponent extends TbHotLinkBaseComponent implements OnDestroy, OnInit, AfterViewInit {
  @ViewChild('combobox') public combobox: any;
  get width(): number { 
    return this.modelComponent.width; 
  }

  comboInputTyping$: Subject<any> = new Subject<any>();
  comboExplicitRequest$: Subject<any> = new Subject<any>();

  constructor(layoutService: LayoutService,
    protected httpService: HttpService,
    protected documentService: DocumentService,
    protected changeDetectorRef: ChangeDetectorRef,
    protected eventDataService: EventDataService,
    public paginatorService: PaginatorService,
    protected filterer: FilterService, 
    protected hyperLinkService: HyperLinkService,
    protected vcr: ViewContainerRef,
    protected mediator: ComponentMediator
  ) {
    super(layoutService, documentService, changeDetectorRef, paginatorService, filterer, hyperLinkService, eventDataService, httpService);
  }

  openDropDown(ev) {
    ev.preventDefault();
    let v = this.modelComponent.model.value;
    this.start();
    this.comboExplicitRequest$.next(v ? v : '');
  }

  closeDropDown() {
    this.stop();
    this.disablePager = true;
  }

  onControlFocusLost: () => void = () => {
    this.eventDataService.change.emit(this.modelComponent.cmpId);
  };

  disablePager: boolean = true;
  get enablePager(): boolean { return !this.disablePager; }

  protected get queryTrigger(): Observable<ServerNeededParams> {
    return this.comboInputTyping$.pipe(debounceFirst(200)).merge(this.comboExplicitRequest$).map(x => ({ model: x }))
  }

  start() {
    this.defaultPageCounter = 0;
    this.filterer.start(200);
    this.paginatorService.start(1, this.pageSize, this.queryTrigger ,this.queryServer);
    this.paginatorService.clientData.subscribe((d) => {
      this.state = this.state.with({ selectionColumn: d.key, gridData: GridData.new({ data: d.rows, total: d.total, columns: d.columns })});
      if (!this.combobox.isOpen) this.combobox.toggle(true);
    });

    this.paginatorService.waiting$.pipe(untilDestroy(this))
      .subscribe(waiting => this.disablePager = (this.paginatorService.isFirstPage && this.paginatorService.noMorePages) || (waiting && this.paginatorService.isJustInitialized));
  }

  protected async pageChange(event: PageChangeEvent) {
    await this.paginatorService.pageChange(event.skip, event.take);
  }

  protected async nextDefaultPage() {
    this.defaultPageCounter++;
    await this.paginatorService.pageChange(this.defaultPageCounter * this.pageSize, this.pageSize);
  }

  protected async prevDefaultPage() {
    this.defaultPageCounter--;
    await this.paginatorService.pageChange(this.defaultPageCounter * this.pageSize, this.pageSize);
  }

  protected async firstDefaultPage() {
    this.defaultPageCounter = 0;
    await this.paginatorService.pageChange(0, this.pageSize);
  }

  onFilterChange(filter: string): void {
    if(this.modelComponent &&  this.modelComponent.model) {
      this.modelComponent.model.value = this.modelComponent.model.uppercase ? filter.toUpperCase() : filter;
      this.comboInputTyping$.next(filter);
    }
  }

  selectionChanged(value: any) {
    _.set(this.eventDataService.model, this.hotLinkInfo.name + '.Description.value', _.get(value, 'displayString'));
    if (this.modelComponent && this.modelComponent.model) {
      this.modelComponent.model.value = _.get(value, 'id');
      this.emitModelChange();
    }
  }

  valueNormalizer = (text$: Observable<string>) => text$.map(text => (new ComboBoxData({ id: text, displayString: '' })));

  setSelectionType(t: string) { this.state = this.state.with({selectionType: t}); }

  ngOnInit() {
    this.mediator.storage.options.componentInfo.cmpId = this.modelComponent.cmpId;
    this.setSelectionType(HotLinkComboSelectionType)
  }

  ngAfterViewInit() {
    TbHotlinkComboHyperLinkHandler.Attach(this);
    TbHotlinkComboEventHandler.Attach(this);
  }

  ngOnDestroy() {
    this.comboInputTyping$.complete();
    this.comboExplicitRequest$.complete();
    this.stop();
  }
}