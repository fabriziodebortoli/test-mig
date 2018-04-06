import { TbComponentService } from './../../../core/services/tbcomponent.service';
import { DocumentService } from './../../../core/services/document.service';
import { EnumsService } from './../../../core/services/enums.service';
import { EventDataService } from './../../../core/services/eventdata.service';
import { LayoutService } from './../../../core/services/layout.service';
import { StorageService } from './../../../core/services/storage.service';
import { ComponentMediator } from './../../../core/services/component-mediator.service';
import { PaginatorService, ServerNeededParams, GridData } from '../../../core/services/paginator.service';
import { FilterService, combineFilters, combineFiltersMap } from '../../../core/services/filter.service';
import { HyperLinkService, HyperLinkInfo } from '../../../core/services/hyperlink.service';
import { HttpService } from './../../../core/services/http.service';

import { GridDataResult, PageChangeEvent, PagerComponent, } from '@progress/kendo-angular-grid';
import { FilterDescriptor, CompositeFilterDescriptor } from '@progress/kendo-data-query';
import { PopupService } from '@progress/kendo-angular-popup';

import { HlComponent, HotLinkState, DefaultHotLinkSelectionType, DescriptionHotLinkSelectionType } from './../hot-link-base/hotLinkTypes';
import { TbHotLinkBaseComponent } from './../hot-link-base/tb-hot-link-base.component';
import { State } from './../../components/customisable-grid/customisable-grid.component';
import { OnDestroy, OnInit, Component, Input, ViewContainerRef, ChangeDetectorRef, ViewChild, ElementRef } from '@angular/core';
import { URLSearchParams } from '@angular/http';

import { HotLinkInfo } from './../../models/hotLinkInfo.model';
import { untilDestroy } from './../../commons/untilDestroy';
import { findAnchestorByClass } from '../../commons/u';

import { TbHotlinkButtonsEventHandler } from './event-handler';
import { TbHotlinkButtonsHyperLinkHandler } from './hyper-link-handler';
import { TbHotlinkButtonsContextMenuHandler } from './context-menu-handler';
import { TbHotlinkButtonsPopupHandler } from './popup';

import { Observable } from '../../../rxjs.imports';
import * as _ from 'lodash';

declare var document: any;

const HotlinkButtonHeight = 16;

@Component({
  selector: 'tb-hotlink-buttons',
  templateUrl: './tb-hot-link-buttons.component.html',
  styleUrls: ['./tb-hot-link-buttons.component.scss'],
  providers: [PaginatorService, FilterService, HyperLinkService, ComponentMediator, StorageService]
})
export class TbHotlinkButtonsComponent extends TbHotLinkBaseComponent implements OnDestroy, OnInit {

  @ViewChild("optionsTemplate") optionsTemplate;
  @ViewChild("tableTemplate") tableTemplate;
  @ViewChild("hotLinkButton") hotLinkButtonTemplate;
  popupHandler: TbHotlinkButtonsPopupHandler;
  buttonIcon = 'tb-hotlink';

  readonly start: () => void = () => {
    this.filterer.start(200, () => this.popupHandler.tablePopupRef.popup.location);
    this.paginatorService.start(1, this.pageSize, this.queryTrigger, this.queryServer);
    this.paginatorService.clientData.subscribe(d => this.state = this.state.with({ selectionColumn: d.key, gridData: GridData.new({ data: d.rows, total: d.total, columns: d.columns }) }));
    this.firstStateChange$.pipe(untilDestroy(this)).subscribe(_ => this.popupHandler.showGridPupup());
  }

  private get firstStateChange$(): Observable<boolean> {
    return this.state$.pipe(untilDestroy(this))
      .skip(1)
      .map(state => state.gridData.columns && state.gridData.columns.length > 0).take(1);
  }

  public get isDisabled(): boolean { if (!this.model) { return true; } return !this.model.enabled; }

  constructor(protected httpService: HttpService,
    protected enumService: EnumsService,
    protected paginatorService: PaginatorService,
    protected filterer: FilterService,
    protected optionsPopupService: PopupService,
    protected tablePopupService: PopupService,
    protected documentService: DocumentService,
    protected vcr: ViewContainerRef,
    protected mediator: ComponentMediator,
    protected hyperLinkService: HyperLinkService) {
    super(mediator.layout, documentService, mediator.changeDetectorRef, paginatorService, filterer, hyperLinkService, mediator.eventData, httpService);
  }

  ngOnInit() {
    this.popupHandler = TbHotlinkButtonsPopupHandler.Attach(this, this.start);
    TbHotlinkButtonsEventHandler.Attach(this);
    TbHotlinkButtonsHyperLinkHandler.Attach(this);
    TbHotlinkButtonsContextMenuHandler.Attach(this);
  }

  search(e?: MouseEvent) { 
    if(e) this.activateHotLinkIcon(e);
    if(this.popupHandler.isTablePopupVisible) this.popupHandler.onHklExit();  
    else this.popupHandler.onSearch();
  }

  selectionTypeChanged(type: string) {
    this.state = this.state.with({ selectionType: type });
    this.popupHandler.closeOptions();
    this.search();
  }

  selectionChanged(value: any) {
    let idx = this.paginatorService.getClientPageIndex(value.index);
    let k = this.state.gridData.data.slice(idx, idx + 1);
    this.value = k[0][this.state.selectionColumn];
    if (this.modelComponent && this.modelComponent.model) {
      this.modelComponent.model.value = this.value;
    }
    let v = _.get(k[0], 'Description');
    _.set(this.eventDataService.model, this.hotLinkInfo.name + '.Description.value', v);
    this.emitModelChange();
    this.popupHandler.closePopups();
  }

  exitButtonClick() {
    this.popupHandler.onHklExit();
  }
  
  private loadOptions() {
    let ns = this.currentHotLinkNamespace;
    if (!ns) ns = this.hotLinkInfo.namespace;
    this.httpService.getHotlinkSelectionTypes(ns).subscribe(json => this.state = this.state.with({ selectionTypes: json.selections }));
  }

  private cursorIsOnHLIconKeyPart(e: MouseEvent) : boolean { return e.offsetY < (HotlinkButtonHeight / 2); }
  public setHotLinkIcon(option: string) {
    if(option === DefaultHotLinkSelectionType) this.buttonIcon = 'tb-hotlinkup';
    else this.buttonIcon = 'tb-hotlinkdown';
  }
  public activateHotLinkIcon(e: MouseEvent) {
    if(this.cursorIsOnHLIconKeyPart(e)) { 
      this.setHotLinkIcon(DefaultHotLinkSelectionType);
      this.state = this.state.with({ selectionType: DefaultHotLinkSelectionType });
    } else {
      this.setHotLinkIcon(DescriptionHotLinkSelectionType);
      this.state = this.state.with({ selectionType: DescriptionHotLinkSelectionType });
    }
  }

  public restoreHotLinkIconToDefault() { 
    setTimeout(() => { 
      this.state = this.state.with({ selectionType: DefaultHotLinkSelectionType }); 
      this.buttonIcon = 'tb-hotlink';
      this.mediator.changeDetectorRef.detectChanges();
    });
  }

  ngOnDestroy() { this.popupHandler.closePopups();  }
}