import { TbComponentService } from './../../../core/services/tbcomponent.service';
import { DocumentService } from './../../../core/services/document.service';
import { EnumsService } from './../../../core/services/enums.service';
import { EventDataService } from './../../../core/services/eventdata.service';
import { LayoutService } from './../../../core/services/layout.service';
import { WebSocketService } from './../../../core/services/websocket.service';
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
import { HyperLinkService, HyperLinkInfo } from '../../../core/services/hyperlink.service';
import { HotLinkInfo } from './../../models/hotLinkInfo.model';
import { untilDestroy } from './../../commons/untilDestroy';
import { HlComponent, HotLinkState } from './../hot-link-base/hotLinkTypes';
import { TriggerData, ComboOpeningSelectionType, ComboFilteringSelectionType, CompleteTriggerDataFactory,
        ValueTriggerDataFactory, NewComboOpeningTriggerData, NewComboFilteringTriggerData } from './../hot-link-base/hotLinkTypes';
import { TbHotlinkComboHyperLinkHandler } from './hyper-link-handler';
import { TbHotlinkComboEventHandler } from './event-handler';

import * as _ from 'lodash';
import { ComboBoxData } from '../../models/combo-box-data';

declare var document: any;
const updateSelectionType: (self: TbHotlinkComboComponent) => (source: Observable<TriggerData>) => Observable<TriggerData> =
  self => source => source.do(t => self.setSelectionType(t.selectionType));

@Component({
  selector: 'tb-hotlink-combo',
  templateUrl: './tb-hot-link-combo.component.html',
  styleUrls: ['./tb-hot-link-combo.component.scss'],
  providers: [PaginatorService, ComponentMediator, StorageService]
})
export class TbHotlinkComboComponent extends TbHotLinkBaseComponent implements OnDestroy, OnInit, AfterViewInit {
  @ViewChild('combobox') public combobox: any;
  get width(): number {
    return this.modelComponent.width;
  }

  private comboInputTypingSubj$: Subject<TriggerData> = new Subject<TriggerData>();
  emitQueryTrigger(t: TriggerData | ValueTriggerDataFactory): void {
    let tdata = 
      (typeof t) === 'function' ? 
      (t as ValueTriggerDataFactory)(this.modelComponent.model.value) : 
      t as TriggerData;

    let eimitter$ = 
      tdata.selectionType === ComboFilteringSelectionType ?
      this.comboInputTypingSubj$ : 
      this.comboExplicitRequestSubj$;

    eimitter$.next(tdata)
  }
  private get comboInputTyping$(): Observable<string>
  { return this.comboInputTypingSubj$.pipe(untilDestroy(this), updateSelectionType(this)).map(t => t.value); }

  private comboExplicitRequestSubj$: Subject<TriggerData> = new Subject<TriggerData>();
  private get comboExplicitRequest$(): Observable<string>
   { return this.comboExplicitRequestSubj$.pipe(untilDestroy(this), updateSelectionType(this)).map(t => t.value); }
  

  constructor(layoutService: LayoutService,
    protected webSocketService: WebSocketService,
    protected httpService: HttpService,
    protected documentService: DocumentService,
    protected changeDetectorRef: ChangeDetectorRef,
    protected eventDataService: EventDataService,
    public paginatorService: PaginatorService,
    protected vcr: ViewContainerRef,
    protected mediator: ComponentMediator
  ) {
    super(layoutService, webSocketService, documentService, changeDetectorRef,
      paginatorService, null, eventDataService, httpService);
  }

  openDropDown(ev) {
    ev.preventDefault();
    //this.emitQueryTrigger(NewComboFilteringTriggerData(this.modelComponent.model.value));
  }

  closeDropDown() {
    this.stop();
    this.disablePager = true;
  }

  protected get queryTrigger$(): Observable<ServerNeededParams> {
    return this.comboInputTyping$.pipe(debounceFirst(200))
            .filter(x => !x.isFirst).map(x => x.value)
            .distinctUntilChanged()
            .merge(this.comboExplicitRequest$)
            .map(value => ({ model: value }));
  }

  disablePager: boolean = true;
  get enablePager(): boolean { return !this.disablePager; }

  start() {
    this.stop();
    this.defaultPageCounter = 0;
    this.paginatorService.start(1, this.pageSize, this.queryTrigger$, this.queryServer);
    this.paginatorService.clientData.subscribe((d) => {
      this.state = this.state.with({ selectionColumn: d.key, gridData: GridData.new({ data: d.rows, total: d.total, columns: d.columns }) });
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
    if (this.modelComponent && this.modelComponent.model) {
      this.modelComponent.model.value = this.modelComponent.model.uppercase ? filter.toUpperCase() : filter;
      this.emitQueryTrigger(NewComboFilteringTriggerData(filter));
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

  setSelectionType(t: string) { this.state = this.state.with({ selectionType: t }); }

  ngOnInit() {
    this.mediator.storage.options.componentInfo.cmpId = this.modelComponent.cmpId;
    this.setSelectionType(ComboOpeningSelectionType)
  }

  ngAfterViewInit() {
    if (this.modelComponent.insideBodyEditDirective === undefined) TbHotlinkComboHyperLinkHandler.Attach(this);
    TbHotlinkComboEventHandler.Attach(this);
  }

  ngOnDestroy() {
    this.comboInputTypingSubj$.complete();
    this.comboExplicitRequestSubj$.complete();
    this.stop();
  }
}