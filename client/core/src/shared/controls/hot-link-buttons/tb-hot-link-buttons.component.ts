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
import { HlComponent, HotLinkState } from './../hot-link-base/hotLinkTypes';
import { untilDestroy } from './../../commons/untilDestroy';
import * as _ from 'lodash';
declare var document: any;

@Component({
  selector: 'tb-hotlink-buttons',
  templateUrl: './tb-hot-link-buttons.component.html',
  styleUrls: ['./tb-hot-link-buttons.component.scss'],
  providers: [PaginatorService, FilterService, HyperLinkService, ComponentMediator, StorageService]
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

  private get hasToAdjustTable(): boolean {
    return (this.adjustTableSub && this.adjustTableSub.closed) || !this.adjustTableSub;
  }
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

  @ViewChild('anchorOptions') optionsButton: ElementRef;

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

  private subscribeOpenTable(anchor, template) {
    if (!this.tableSub)
      this.tableSub = this.showTable$.distinctUntilChanged().pipe(untilDestroy(this)).subscribe(hasToOpen => {
        if (hasToOpen) {
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
          if (this.tablePopupRef.popupElement) {
            this.oldTablePopupZIndex = this.tablePopupRef.popupElement.style.zIndex;
            this.tablePopupRef.popupElement.style.maxWidth = PopupHelper.getScaledDimension(1000);
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
            appendTo: this.vcr
          });
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

  public getOptionClass(item: any): any {
    return { elemSelected: item === this.state.selectionType, selTypeElem: true };
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
        if (args.customFilters && args.customFilters.logic && args.customFilters.filters)
          p.set('customFilters', JSON.stringify(args.customFilters));
        if (args.customSort)
          p.set('customSort', JSON.stringify(args.customSort));
        p.set('disabled', '0');
        p.set('page', JSON.stringify(pageNumber + 1));
        p.set('per_page', JSON.stringify(serverPageSize));
        return this.httpService.getHotlinkData(ns, this.state.selectionType, p);
      });

    if (this.hasToAdjustTable) this.adjustTable$.pipe(untilDestroy(this)).subscribe(_ => this.adjustTablePopupGrid());

    this.paginator.clientData.subscribe((d) => {
      this.state = this.state.with({ selectionColumn: d.key, gridData: GridData.new({ data: d.rows, total: d.total, columns: d.columns }) });
    });
  }

  private adjustTablePopupGrid(): void {
    if (!this.tablePopupRef) return;
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
    this.state = this.state.with({ selectionType: type });
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
    this.state.with({ selectionTypes: json.selections });
  }

  shouldAddOnFly = (focusedElem: HTMLElement) => !this.optionsButton.nativeElement.contains(focusedElem);
 
  ngOnInit() {
    // fix for themes css conflict in form.scss style 
    if (this.modelComponent) {
      this.mediator.storage.options.componentInfo.cmpId = this.modelComponent.cmpId;
      let hyperLinkElem = (this.vcr.element.nativeElement.parentNode.getElementsByClassName('k-textbox') as HTMLCollection).item(0) as HTMLElement;
      this.hyperLinkService.start(hyperLinkElem,
        {
          name: this.hotLinkInfo.name,
          cmpId: this.documentService.mainCmpId,
          enableAddOnFly: this.hotLinkInfo.enableAddOnFly,
          mustExistData: this.hotLinkInfo.mustExistData,
          model: this.modelComponent.model
        },
        this.slice$,
        this.clearModel,
        this.afterAddOnFly,
        this.shouldAddOnFly);
    }

    this.adjustTableSub = this.adjustTable$.pipe(untilDestroy(this)).subscribe(_ => this.adjustTablePopupGrid());

    Observable.fromEvent(document, 'keyup', { capture: true })
      .pipe(untilDestroy(this))
      .filter(e => (e as any).keyCode === 27)
      .subscribe(e => this.closePopups());

    Observable.fromEvent(document, 'click', { capture: true }).pipe(untilDestroy(this))
      .filter(e => ((this.tablePopupRef && !this.tablePopupRef.popupElement.contains((e as any).toElement))
        || (this.optionsPopupRef && !this.optionsPopupRef.popupElement.contains((e as any).toElement)))
        && (this.isTablePopupVisible || this.isOptionsPopupVisible))
      .subscribe(_ => setTimeout(() => this.closePopups()));
  }

  ngOnDestroy() {
    this.closePopups();
  }
}