import { TbComponentService } from './../../../core/services/tbcomponent.service';
import { DocumentService } from './../../../core/services/document.service';
import { EnumsService } from './../../../core/services/enums.service';
import { EventDataService } from './../../../core/services/eventdata.service';
import { LayoutService } from './../../../core/services/layout.service';
import { StorageService } from './../../../core/services/storage.service';
import { ComponentMediator } from './../../../core/services/component-mediator.service';
import { TbHotLinkBaseComponent } from './../hot-link-base/tb-hot-link-base.component';
import { State } from './../../components/customisable-grid/customisable-grid.component';
import { HttpService } from './../../../core/services/http.service';
import { OnDestroy, OnInit, Component, Input, ViewContainerRef, ChangeDetectorRef, ViewChild, ElementRef } from '@angular/core';
import { URLSearchParams } from '@angular/http';
import { GridDataResult, PageChangeEvent, PagerComponent, } from '@progress/kendo-angular-grid';
import { FilterDescriptor, CompositeFilterDescriptor } from '@progress/kendo-data-query';
import { PopupHelper } from './popup';
import { PopupService, PopupSettings, PopupRef } from '@progress/kendo-angular-popup';

import { BehaviorSubject, Subscription, Observable, Subject } from '../../../rxjs.imports';
import { PaginatorService, ServerNeededParams, GridData } from '../../../core/services/paginator.service';
import { FilterService, combineFilters, combineFiltersMap } from '../../../core/services/filter.services';
import { HyperLinkService, HyperLinkInfo } from '../../../core/services/hyperlink.service';
import { HotLinkInfo } from './../../models/hotLinkInfo.model';
import { HlComponent, HotLinkState, DefaultHotLinkSelectionType } from './../hot-link-base/hotLinkTypes';
import { untilDestroy } from './../../commons/untilDestroy';
import * as _ from 'lodash';
import { findAnchestorByClass } from '../../commons/u';
declare var document: any;

type OpenCloseTableFunction = () => void;
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

  private _optionOffset: {top: number, left: number};
  private optionsPopupRef: PopupRef;
  private tablePopupRef: PopupRef;
  private optionsSub: Subscription;
  private gridAutofitSub: Subscription;
  private isTablePopupVisible = false;
  private declareTablePopupVisible: () => void = () => { this.isTablePopupVisible = true; } 
  private declareTablePopupHidden: () => void = () => { this.isTablePopupVisible = false; } 
  private isOptionsPopupVisible = false;
  private declareOptionsPopupVisible: () => void = () => { this.isOptionsPopupVisible = true; } 
  private declareOptionsPopupHidden: () => void = () => { this.isOptionsPopupVisible = false; } 
  private previousNext = true;

  private get firstStateChange$(): Observable<any> {
    return this.state$.pipe(untilDestroy(this)).filter(state => state.gridData.columns && state.gridData.columns.length > 0 &&  this.tablePopupRef !== undefined).take(1);
  }

  closeOptions() { this.showOptionsSubj$.next(false); } 
  openOptions() { this.showOptionsSubj$.next(true); }
  closeTable: OpenCloseTableFunction = () => { this.closeOptions(); this.showTableSubj$.next(false); this.stop(); }
  openTable: OpenCloseTableFunction = () => this.showTableSubj$.next(true);
  closePopups() { this.closeOptions(); this.closeTable(); }

  private showTableSubj$ = new BehaviorSubject(false);
  public get showTable$(): Observable<boolean> { return this.showTableSubj$.filter(hasToOpen => hasToOpen).map(_ => true); }
  public get hideTable$(): Observable<boolean> { return this.showTableSubj$.filter(hasToOpen => !hasToOpen).map(_ => false); }

  private showOptionsSubj$ = new BehaviorSubject(false);
  public get showOptions$() { return this.showOptionsSubj$.filter(hasToOpen => hasToOpen).map(_ => true); }
  public get hideOptions$() { return this.showOptionsSubj$.filter(hasToOpen => !hasToOpen).map(_ => false); }

  public get isDisabled(): boolean { if (!this.model) { return true; } return !this.model.enabled; }

  private oldTablePopupZIndex: string;
  buttonIcon = 'tb-hotlink';

  constructor(protected httpService: HttpService,
    protected documentService: DocumentService,
    protected enumService: EnumsService,
    protected paginator: PaginatorService,
    protected filterer: FilterService,
    protected hyperLinkService: HyperLinkService,
    protected optionsPopupService: PopupService,
    protected tablePopupService: PopupService,
    protected vcr: ViewContainerRef,
    protected mediator: ComponentMediator) {
    super(mediator.layout, documentService, mediator.changeDetectorRef, paginator, filterer, hyperLinkService, mediator.eventData);
  }

  private setPopupElementInBackground() {
    if (this.tablePopupRef.popupElement) {
      this.oldTablePopupZIndex = this.tablePopupRef.popupElement.style.zIndex;
      this.tablePopupRef.popupElement.style.maxWidth = PopupHelper.getScaledDimension(1000);
      this.tablePopupRef.popupElement.style.zIndex = '-1';
    }
  }

  private setPopupElementInForeground() {
    this.tablePopupRef.popupElement.style.zIndex = this.oldTablePopupZIndex;
  }

  public getOptionClass(item: any): any { return { selTypeElem: true }; }

  toggleTable(e?: MouseEvent) { 
    if(e) this.activateHotLinkIcon(e);
    if(this.isTablePopupVisible) this.closeTable();  
    else this.openTable();
  }
  get optionsPopupStyle(): any { return { 'background': 'whitesmoke', 'border': '1px solid rgba(0,0,0,.05)' }; }

  private get queryTrigger() : Observable<ServerNeededParams> {
    return Observable.combineLatest(this.slice$, this.filterer.filterChanged$, this.filterer.sortChanged$,
      (slice, filter, sort) => ({ model: slice, customFilters: filter, customSort: sort }))
  } 

  private get 
  protected start() {
    this.defaultPageCounter = 0;
    this.filterer.start(200, this.tablePopupRef.popup.location);
    this.paginator.start(1, this.pageSize,
      this.queryTrigger,
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
        if (args.customFilters && args.customFilters.logic && args.customFilters.filters)
          p.set('customFilters', JSON.stringify(args.customFilters));
        if (args.customSort)
          p.set('customSort', JSON.stringify(args.customSort));
        p.set('disabled', '0');
        p.set('page', JSON.stringify(pageNumber + 1));
        p.set('per_page', JSON.stringify(serverPageSize));
        return this.httpService.getHotlinkData(ns, this.state.selectionType, p);
      });

      
      this.paginator.clientData.subscribe((d) => {
        this.state = this.state.with({ selectionColumn: d.key, gridData: GridData.new({ data: d.rows, total: d.total, columns: d.columns }) });
      });
      this.firstStateChange$.pipe(untilDestroy(this)).subscribe(_ => this.adjustTablePopupGrid());
  }


  private showGridFilters() {
    if (!this.tablePopupRef) return;
    let toggleFiltersBtn = ((this.tablePopupRef.popupElement.getElementsByClassName('tb-toggle-filters') as HTMLCollection).item(0) as HTMLElement);
    if(!toggleFiltersBtn) return;
    toggleFiltersBtn.click();
  }

  private autofitGridColumns() {
    let autofitColumnsBtn = ((this.tablePopupRef.popupElement
      .getElementsByClassName('tb-autofit-columns') as HTMLCollection).item(0) as HTMLElement);
    if (autofitColumnsBtn) autofitColumnsBtn.click();
    this.setPopupElementInForeground();
  }

  private adjustTablePopupGrid(): void {
     this.showGridFilters();
      setTimeout(() => { 
        this.autofitGridColumns();
        this.declareTablePopupVisible();
      });
  }

  selectionTypeChanged(type: string) {
    this.state = this.state.with({ selectionType: type });
    this.closeOptions();
    this.toggleTable();
    this.state = this.state.with({ selectionType: DefaultHotLinkSelectionType });
  }

  selectionChanged(value: any) {
    let idx = this.paginator.getClientPageIndex(value.index);
    let k = this.state.gridData.data.slice(idx, idx + 1);
    this.value = k[0][this.state.selectionColumn];
    if (this.modelComponent && this.modelComponent.model) {
      this.modelComponent.model.value = this.value;
    }
    let v = _.get(k[0], 'Description');
    _.set(this.eventDataService.model, this.hotLinkInfo.name + '.Description.value', v);
    this.emitModelChange();
    this.closePopups();
  }
  
  ngOnInit() {
    this.withCloseOnEscLogic()
      .withClickHandlingLogic()
      .withHyperLinkLogic()
      .withContextMenuLogic()
      .withOpenCloseTableLogic();
  }

  private loadTable() { this.start(); }

  private async loadOptions() {
    let ns = this.currentHotLinkNamespace;
    if (!ns) ns = this.hotLinkInfo.namespace;
    let json = await this.httpService.getHotlinkSelectionTypes(ns).toPromise();
    this.state = this.state.with({ selectionTypes: json.selections });
  }


  private withCloseOnEscLogic(): TbHotlinkButtonsComponent {
    Observable.fromEvent(document, 'keyup', { capture: true })
    .pipe(untilDestroy(this))
    .filter(e => (e as any).keyCode === 27)
    .subscribe(e => this.closePopups());
    return this;
  }

  private withClickHandlingLogic(): TbHotlinkButtonsComponent {
    Observable.fromEvent(document, 'click', { capture: true }).pipe(untilDestroy(this))
    .filter(e => ((this.tablePopupRef && !this.tablePopupRef.popupElement.contains((e as any).toElement) 
        && !findAnchestorByClass(e['target'], 'customisable-grid-filter')
        && !findAnchestorByClass(e['target'], 'k-calendar-view')
        && !findAnchestorByClass(e['target'], 'k-popup'))
      || (this.optionsPopupRef 
        && !this.optionsPopupRef.popupElement.contains((e as any).toElement)))
      && (this.isTablePopupVisible || this.isOptionsPopupVisible))
    .subscribe(_ => setTimeout(() => this.closePopups()));
    return this;
  }

  private getHotLinkElement: () => HTMLElement = () => (this.vcr.element.nativeElement.parentNode.getElementsByClassName('k-textbox') as HTMLCollection).item(0) as HTMLElement;

  private withHyperLinkLogic(): TbHotlinkButtonsComponent {
    // fix for themes css conflict in form.scss style 
    if (this.modelComponent) {
      this.mediator.storage.options.componentInfo.cmpId = this.modelComponent.cmpId;
      this.hyperLinkService.start(
        this.getHotLinkElement,
        {
          name: this.hotLinkInfo.name,
          cmpId: this.documentService.mainCmpId,
          enableAddOnFly: this.hotLinkInfo.enableAddOnFly,
          mustExistData: this.hotLinkInfo.mustExistData,
          model: this.modelComponent.model
        },
        this.slice$,
        this.afterNoAddOnFly,
        this.afterAddOnFly);
    }
    return this;
  }

  private withContextMenuLogic(): TbHotlinkButtonsComponent {
    Observable.fromEvent<MouseEvent>(this.hotLinkButtonTemplate.nativeElement, 'contextmenu', { capture: true }).pipe(untilDestroy(this))
    .do(e => e.preventDefault())
    .do(_ => this.closeTable())
    .pipe(untilDestroy(this))
    .subscribe(e => {
      this._optionOffset = {top: e.clientY, left: e.clientX};
      if(this.isOptionsPopupVisible) this.closeOptions();
      else this.openOptions()
    });
    return this;
  }

  private cursorIsOnHLIconKeyPart(e: MouseEvent) : boolean { return e.offsetY < (HotlinkButtonHeight / 2); }
  private activateHotLinkIcon(e: MouseEvent) {
    if(this.cursorIsOnHLIconKeyPart(e)) { 
      this.buttonIcon = 'tb-hotlinkup';
      this.state = this.state.with({ selectionType: DefaultHotLinkSelectionType });
    } else {
      this.buttonIcon = 'tb-hotlinkdown';
      this.state = this.state.with({ selectionType: 'description' });
    }
  }

  private restoreHotLinIconToDefault() { 
    setTimeout(() => { 
      this.state = this.state.with({ selectionType: DefaultHotLinkSelectionType }); 
      this.buttonIcon = 'tb-hotlink';
      this.mediator.changeDetectorRef.detectChanges();
    });
  }

  private withOpenCloseTableLogic(): TbHotlinkButtonsComponent {
      this.showTable$.pipe(untilDestroy(this)).subscribe(_ => {
        let popupA = PopupHelper.getPopupAlign(this.hotLinkButtonTemplate.nativeElement);
        let anchorA = PopupHelper.getAnchorAlign(this.hotLinkButtonTemplate.nativeElement);
        this.tablePopupRef = this.tablePopupService.open({
          anchor: this.hotLinkButtonTemplate.nativeElement,
          content: this.tableTemplate,
          popupAlign: popupA,
          anchorAlign: anchorA,
          popupClass: 'tb-hotlink-tablePopup',
          appendTo: this.vcr});
        this.setPopupElementInBackground();
        this.tablePopupRef.popupOpen.asObservable()
          .subscribe(_ => this.loadTable());
      });

      this.hideTable$.pipe(untilDestroy(this)).subscribe(_ => {
        if (this.tablePopupRef) {
          this.tablePopupRef.close();
          this.tablePopupRef = null;
        }
        this.declareTablePopupHidden();
        this.restoreHotLinIconToDefault();
      });

      this.showOptions$.pipe(untilDestroy(this)).subscribe(_ => {
        this.optionsPopupRef = this.optionsPopupService.open({
          content: this.optionsTemplate,
          appendTo: this.vcr,
          offset: this._optionOffset
        });
        this.optionsPopupRef.popupOpen.asObservable()
          .do(_ => this.declareOptionsPopupVisible())
          .subscribe(_ => this.loadOptions());
      });
    
      this.hideOptions$.pipe(untilDestroy(this)).subscribe(_ => {
        if (this.optionsPopupRef) { this.optionsPopupRef.close(); }
          this.optionsPopupRef = null;
          this.declareOptionsPopupHidden();
      });

      return this;
  }

  ngOnDestroy() { this.closePopups();  }
}