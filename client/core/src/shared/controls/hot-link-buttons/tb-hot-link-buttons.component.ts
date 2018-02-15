import { TbComponentService } from './../../../core/services/tbcomponent.service';
import { DocumentService } from './../../../core/services/document.service';
import { EnumsService } from './../../../core/services/enums.service';
import { EventDataService } from './../../../core/services/eventdata.service';
import { LayoutService } from './../../../core/services/layout.service';
import { TbHotLinkBaseComponent } from './../hot-link-base/tb-hot-link-base.component';
import { State } from './../../components/customisable-grid/customisable-grid.component';
import { HttpService } from './../../../core/services/http.service';
import { OnDestroy, OnInit, Component, Input, ViewContainerRef, ChangeDetectorRef } from '@angular/core';
import { URLSearchParams } from '@angular/http';
import { GridDataResult, PageChangeEvent, PagerComponent,  } from '@progress/kendo-angular-grid';
import { FilterDescriptor, CompositeFilterDescriptor } from '@progress/kendo-data-query';
import { PopupHelper } from './popup';
import { PopupService, PopupSettings, PopupRef } from '@progress/kendo-angular-popup';
import { BehaviorSubject, Subscription, Observable } from '../../../rxjs.imports';
import { PaginatorService, ServerNeededParams } from '../../../core/services/paginator.service';
import { FilterService, combineFilters, combineFiltersMap } from '../../../core/services/filter.services';
import { HyperLinkService, HyperLinkInfo} from '../../../core/services/hyperlink.service';
import { HotLinkInfo } from './../../models/hotLinkInfo.model';
import { HlComponent, HotLinkState } from './../hot-link-base/hotLinkTypes';
import { untilDestroy } from './../../commons/untilDestroy';
import * as _ from 'lodash';
declare var document:any;

@Component({
  selector: 'tb-hotlink-buttons',
  templateUrl: './tb-hot-link-buttons.component.html',
  styleUrls: ['./tb-hot-link-buttons.component.scss'],
  providers: [PaginatorService, FilterService, HyperLinkService]
})
export class TbHotlinkButtonsComponent extends TbHotLinkBaseComponent implements OnDestroy, OnInit {


  private optionsPopupRef: PopupRef;
  private tablePopupRef: PopupRef;
  private optionsSub: Subscription;
  private tableSub: Subscription;
  private gridAutofitSub: Subscription;
  private isTablePopupVisible = false;
  private isOptionsPopupVisible = false;

  private previousNext = true;

  private adjustTableSub: Subscription;
  private get adjustTable$(): Observable<any> {
    return this.state$.pipe(untilDestroy(this))
    .filter(_ => this.tablePopupRef !== undefined)
    .take(1);
  }

  private showTableSubj$ = new BehaviorSubject(false);
  public get showTable$(): Observable<boolean> { return this.showTableSubj$.asObservable(); }
  
  private showOptionsSubj$ = new BehaviorSubject(false);
  public get showOptions$() { return this.showOptionsSubj$.asObservable(); }

  public get isDisabled(): boolean { if (!this.model) { return true; } return !this.model.enabled; }

  private oldTablePopupZIndex: string;
  private _tbInfo: {element: HTMLElement, clickSubscription?: Subscription, initInfo: { color: string, textDecoration: string, cursor: string, pointerEvents: string }}
  get textBoxInfo (): {element: HTMLElement, clickSubscription?: Subscription, initInfo: { color: string, textDecoration: string, cursor: string, pointerEvents: string }}  {
    if(this._tbInfo) return this._tbInfo;

    let tb: HTMLElement;
    tb = (this.vcr.element.nativeElement.parentNode.getElementsByClassName('k-textbox') as HTMLCollection).item(0) as HTMLElement;
    if(!tb) return undefined;
    let oldColor = tb.style.color;
    let oldDecoration = tb.style.textDecoration;
    let oldCursor = tb.style.cursor;
    let oldPointerEvents = tb.style.pointerEvents;
    this._tbInfo = { 
        element: tb,
        initInfo: {
          color: oldColor,
          textDecoration: oldDecoration,
          cursor: oldCursor,
          pointerEvents: oldPointerEvents
        }
    };
    return this._tbInfo;
  }

  constructor(layoutService: LayoutService,
              protected httpService: HttpService,
              protected documentService: DocumentService,
              protected enumService: EnumsService,
              protected changeDetectorRef: ChangeDetectorRef,
              protected eventDataService: EventDataService,
              protected paginator: PaginatorService,
              protected filterer: FilterService,
              protected hyperLinkService: HyperLinkService,
              protected optionsPopupService: PopupService,
              protected tablePopupService: PopupService,
              protected vcr: ViewContainerRef) {
    super(layoutService, documentService, changeDetectorRef, paginator, filterer, hyperLinkService, eventDataService);
  }

  private subscribeOpenTable(anchor, template){
    if(!this.tableSub)
      this.tableSub = this.showTable$.distinctUntilChanged().pipe(untilDestroy(this)).subscribe(hasToOpen => {
      if(hasToOpen){
        let popupA = PopupHelper.getPopupAlign(anchor);
        let anchorA = PopupHelper.getAnchorAlign(anchor);
        this.tablePopupRef = this.tablePopupService.open({
          anchor: anchor,
          content: template,
          popupAlign: popupA,
          anchorAlign: anchorA,
          popupClass: 'tb-hotlink-tablePopup',
          appendTo: this.vcr
         });
        if(this.tablePopupRef.popupElement) {
          this.oldTablePopupZIndex = this.tablePopupRef.popupElement.style.zIndex;
          this.tablePopupRef.popupElement.style.maxWidth = JSON.stringify(document.body.offsetWidth) + 'px';
          this.tablePopupRef.popupElement.style.zIndex = '-1'; 
        }
        this.tablePopupRef.popupOpen.asObservable()
          .subscribe(_ => this.loadTable());
      } else {
        if (this.tablePopupRef) { 
          this.tablePopupRef.close();
          this.tablePopupRef = null; 
        }
        this.isTablePopupVisible = false;
      }
    });
  }

  private subscribeOpenOptions(anchor, template) {
    if (!this.optionsSub) {
      this.optionsSub = this.showOptions$.distinctUntilChanged().pipe(untilDestroy(this)).subscribe(hasToOpen => {
        if (hasToOpen) {
          this.optionsPopupRef = this.optionsPopupService.open({ 
            anchor: anchor,
            content: template,
            popupClass: 'tb-hotlink-optionsPopup',
            appendTo: this.vcr });
          this.optionsPopupRef.popupOpen.asObservable()
            .do(_ => this.isOptionsPopupVisible = true)
            .subscribe(_ => this.loadOptions());
        } else {
          if (this.optionsPopupRef) { this.optionsPopupRef.close(); }
            this.optionsPopupRef = null;
            this.isOptionsPopupVisible = false;
        }
      });
    }
  }

  toggleTable(anchor, template) {
    this.closeOptions();
    if (this.showTableSubj$.value) { this.closeTable(); } 
    else { this.subscribeOpenTable(anchor, template); this.openTable(); }
  }

  toggleOptions(anchor, template) {
    this.closeTable();
    if (this.showOptionsSubj$.value) { this.closeOptions(); } 
    else { this.subscribeOpenOptions(anchor, template); this.openOptions(); }
  }

  closeOptions() { this.showOptionsSubj$.next(false); }
  openOptions() { this.showOptionsSubj$.next(true); }
  closeTable() { this.showTableSubj$.next(false); this.stop(); }
  openTable() { this.showTableSubj$.next(true); }
  closePopups() { this.closeOptions(); this.closeTable(); }
  get optionsPopupStyle(): any { return { 'background': 'whitesmoke', 'border': '1px solid rgba(0,0,0,.05)' }; }
  protected start() {
    this.defaultPageCounter = 0;
    this.filterer.start(200);
    this.paginator.start(1, this.pageSize,
      Observable
      .combineLatest(this.slice$, this.filterer.filterChanged$, this.filterer.sortChanged$,
        (slice, filter, sort) => ({ model: slice, customFilters: filter, customSort: sort})),
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
        return this.httpService.getHotlinkData(ns, this.state.selectionType,  p);
      });

     if(this.adjustTableSub && this.adjustTableSub.closed)
       this.adjustTable$.pipe(untilDestroy(this)).subscribe(_ => this.adjustTablePopupGrid());

    this.paginator.clientData.subscribe((d) => {
        this.state = {...this.state, selectionColumn: d.key, gridData: { data: d.rows, total: d.total, columns: d.columns} };        
    });
  }

  private adjustTablePopupGrid(): void {
    if(!this.tablePopupRef) return;
    let toggleFiltersBtn = ((this.tablePopupRef.popupElement.getElementsByClassName('tb-toggle-filters') as HTMLCollection).item(0) as HTMLElement);
    if (toggleFiltersBtn) {
      toggleFiltersBtn.click();
      setTimeout(() => {
        let autofitColumnsBtn = ((this.tablePopupRef.popupElement
          .getElementsByClassName('tb-autofit-columns') as HTMLCollection).item(0) as HTMLElement);
        if (autofitColumnsBtn) autofitColumnsBtn.click();
        this.tablePopupRef.popupElement.style.zIndex = this.oldTablePopupZIndex;
        this.isTablePopupVisible = true
      });
    }
  }

  private loadTable() { this.start(); }

  selectionTypeChanged(type: string) {
    this.state = {...this.state, selectionType: type };
    this.closeOptions();
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

  private async loadOptions() {
    let ns = this.currentHotLinkNamespace;
    if (!ns) ns = this.hotLinkInfo.namespace;
    let json = await this.httpService.getHotlinkSelectionTypes(ns).toPromise();
    this.state = {...this.state, selectionTypes: json.selections };
  }

  ngOnInit() {
    // fix for themes css conflict in form.scss style 
    if(this.modelComponent){
      if(!this.modelComponent.width) {
        if (this.textBoxInfo) { this.textBoxInfo.element.style.width = 'auto'; }
      }

      if (this.textBoxInfo){
        this.slice$.pipe(untilDestroy(this)).subscribe(x => {
          if(!x.enabled && x.value) { 
            this.textBoxInfo.element.style.textDecoration = 'underline'; 
            this.textBoxInfo.element.style.color = 'blue';
            this.textBoxInfo.element.style.cursor = 'pointer';
            this.textBoxInfo.element.style.pointerEvents = 'all';
            this.textBoxInfo.clickSubscription = Observable.fromEvent(document, 'click', { capture: false })
              .filter(e => this.textBoxInfo &&  (e as any) && (e as any).target === this.textBoxInfo.element)
              .subscribe(e => this.hyperLinkService.follow({ns: this.hotLinkInfo.name, cmpId: this.documentService.mainCmpId }));
          } else {
            this.textBoxInfo.element.style.textDecoration = this.textBoxInfo.initInfo.textDecoration;
            this.textBoxInfo.element.style.color = this.textBoxInfo.initInfo.color;
            this.textBoxInfo.element.style.cursor = this.textBoxInfo.initInfo.cursor;
            this.textBoxInfo.element.style.pointerEvents = this.textBoxInfo.initInfo.pointerEvents;
            if(this.textBoxInfo.clickSubscription)
              this.textBoxInfo.clickSubscription.unsubscribe();
          } 
        });
      }
    }

    this.adjustTableSub = this.adjustTable$.pipe(untilDestroy(this)).subscribe(_ => this.adjustTablePopupGrid());

    Observable.fromEvent(document, 'keyup', { capture: true })
    .pipe(untilDestroy(this))
    .filter(e => (e as any).keyCode === 27)
    .subscribe(e => this.closePopups());
    
    Observable.fromEvent(document, 'click', {capture: true}).pipe(untilDestroy(this))
    .filter(e => ((this.tablePopupRef && !this.tablePopupRef.popupElement.contains((e as any).toElement))
                  || (this.optionsPopupRef && !this.optionsPopupRef.popupElement.contains((e as any).toElement)))
              && (this.isTablePopupVisible || this.isOptionsPopupVisible))
    .subscribe(_ => setTimeout(() => this.closePopups()));
  }

  ngOnDestroy() {
    this.closePopups();
  }
}