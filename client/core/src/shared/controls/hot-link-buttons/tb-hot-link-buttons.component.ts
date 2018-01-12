import { TbComponentService } from './../../../core/services/tbcomponent.service';
import { EnumsService } from './../../../core/services/enums.service';
import { EventDataService } from './../../../core/services/eventdata.service';
import { LayoutService } from './../../../core/services/layout.service';
import { ControlComponent } from './../control.component';
import { HttpService } from './../../../core/services/http.service';
import { OnDestroy, OnInit, Component, Input, HostListener, ElementRef, ViewContainerRef,
  ChangeDetectionStrategy, ChangeDetectorRef, NgZone, ViewEncapsulation } from '@angular/core';
import { URLSearchParams } from '@angular/http';
import { GridDataResult, PageChangeEvent, PagerComponent,  } from '@progress/kendo-angular-grid';
import { filterBy, FilterDescriptor, CompositeFilterDescriptor } from '@progress/kendo-data-query';
import { PopupService, PopupSettings, PopupRef, Align } from '@progress/kendo-angular-popup';
import { BehaviorSubject, Subscription, Observable } from '../../../rxjs.imports';
import { PaginatorService, ServerNeededParams } from '../../../core/services/paginator.service';
import { FilterService, combineFilters } from '../../../core/services/filter.services';
import { HotLinkInfo } from './../../models/hotLinkInfo.model';
import * as _ from 'lodash';

export type HlComponent = { width?: number, model: any, slice$?: any, cmpId: string, isCombo?: boolean, hotLink: HotLinkInfo };

@Component({
  selector: 'tb-hotlink-buttons',
  templateUrl: './tb-hot-link-buttons.component.html',
  styleUrls: ['./tb-hot-link-buttons.component.scss'],
  providers: [PaginatorService, FilterService],
  changeDetection: ChangeDetectionStrategy.Default
})
export class TbHotlinkButtonsComponent extends ControlComponent implements OnDestroy, OnInit {

  private _modelComponent: HlComponent
  @Input() public get modelComponent(): HlComponent {
    return this._modelComponent;
  }

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

  private gridView$ = new BehaviorSubject<{data: any[], total: number, columns: any[] }>
  ({data: [], total: 0, columns: [] });
  public columns: any[];
  public selectionTypes: any[] = [];
  public selectionType = 'code';

  private info = false;
  private type: 'numeric' | 'input' = 'numeric';
  private pageSizes = false;
  private previousNext = true;
  private pageSize = 20;
  private showTableSubj$ = new BehaviorSubject(false);
  private defaultPageCounter = 0;

  private _filter: CompositeFilterDescriptor;
  private get filter(): CompositeFilterDescriptor {
    return this._filter;
  }

  private set filter(value: CompositeFilterDescriptor) {
    this._filter = _.cloneDeep(value);
    this.filterer.filter = _.cloneDeep(value);
    this.changedFilterIndex = this.columns.findIndex(c => c.id === this.filterer.changedField);
    this.filterer.onFilterChanged(value);
  }

  public get showTable$(): Observable<boolean> {
    return this.showTableSubj$.asObservable();
  }

  private showOptionsSubj$ = new BehaviorSubject(false);
  public get showOptions$() {
    return this.showOptionsSubj$.asObservable();
  }

  public get isDisabled(): boolean {
    if (!this.model) { return true; }
    return !this.model.enabled;
  }

  selectionColumn = '';
  subscription: Subscription;

  _defaultGridStyle = {'cursor': 'pointer'};
  _filterTypingGridStyle = {'color': 'darkgrey'}
  _gridStyle$ = new BehaviorSubject<any>(this._defaultGridStyle);
  get gridStyle$(): Observable<any> { return this._gridStyle$.asObservable(); }

  private changedFilterIndex = 0;

  private optionsPopupRef: PopupRef;
  optionsSub: Subscription;

  private tablePopupRef: PopupRef;
  tableSub: Subscription;
  currentHotLinkNamespace: string;
  private isTablePopupVisible = false;
  private isOptionsPopupVisible = false;

  constructor(public httpService: HttpService,
    layoutService: LayoutService,
    public enumService: EnumsService,
    tbComponentService: TbComponentService,
    changeDetectorRef: ChangeDetectorRef,
    private eventDataService: EventDataService,
    private paginator: PaginatorService,
    private filterer: FilterService,
    private ngZone: NgZone,
    private elRef: ElementRef,
    private optionsPopupService: PopupService,
    private tablePopupService: PopupService,
    private vcr: ViewContainerRef
  ) {
    super(layoutService, tbComponentService, changeDetectorRef);
  }

  get isAttachedToAComboBox(): boolean {
    if (this.modelComponent && this.modelComponent.isCombo) { return true; }
    return false;
  }

  private subscribeOpenTable(anchor, template){
    if(!this.tableSub)
      this.tableSub = this.showTable$.distinctUntilChanged().subscribe(hasToOpen => {
      if(hasToOpen){
        let popupA = this.getPopupAlign(anchor);
        let anchorA = this.getAnchorAlign(anchor);
        this.tablePopupRef = this.tablePopupService.open({anchor: anchor, content: template, popupAlign: popupA, anchorAlign: anchorA });
        this.tablePopupRef.popupOpen.asObservable()
          .do(_ => this.isTablePopupVisible = true)
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
      this.optionsSub = this.showOptions$.distinctUntilChanged().subscribe(hasToOpen => {
        if (hasToOpen) {
          this.optionsPopupRef = this.optionsPopupService.open({ anchor: anchor, content: template });
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

   @HostListener('keydown', ['$event'])
   public keydown(event: any): void {
     if (event.keyCode === 27) { this.closePopups(); }
   }
 
  @HostListener('document:click', ['$event'])
  public documentClick(event: any): void {
    if (this.isTablePopupVisible && !this.tablePopupRef.popupElement.contains(event.toElement)) {
      this.closeTable();
    }
    if (this.isOptionsPopupVisible && !this.optionsPopupRef.popupElement.contains(event.toElement)) {
      this.closeOptions();
    }  
  }

  getPopupAlign(anchor: any): Align {
    let Horizontal = 'left';
    let Vertical = 'top';

    let height = window.innerHeight;
    let bottomPoint = anchor.offsetTop + anchor.offsetHeight;
    if (height - bottomPoint <= 300) {
      Vertical = 'bottom';
    }

    let width = window.innerWidth;
    let rightPoint = anchor.offsetLeft + anchor.offsetWidth;
    if (rightPoint > (width *0.45)) {
      Horizontal = 'right';
    }
    return {
      horizontal: Horizontal,
      vertical: Vertical
    } as Align;

  }

  getAnchorAlign(anchor: any): Align {
    let Horizontal = 'left';
    let Vertical = 'bottom';

    let height = window.innerHeight;
    let bottomPoint = anchor.offsetTop + anchor.offsetHeight;
    if (height - bottomPoint <= 300) {
      Vertical = 'top';
    }

    let width = window.innerWidth;
    let rightPoint = anchor.offsetLeft + anchor.offsetWidth;
    if (rightPoint > (width *0.45)) {
      Horizontal = 'right';
    }

    return {
      horizontal: Horizontal,
      vertical: Vertical
    } as Align;
  }

  openDropDown() {
    this.start();
  }

  closeDropDown() {
    this.stop();
  }

  closeOptions() { this.showOptionsSubj$.next(false); }
  openOptions() { this.showOptionsSubj$.next(true); }
  closeTable() { this.showTableSubj$.next(false); this.stop(); }
  openTable() { this.showTableSubj$.next(true); }
  closePopups() { this.closeOptions(); this.closeTable(); }
  get optionsPopupStyle(): any { return { 'background': 'whitesmoke', 'border': '1px solid rgba(0,0,0,.05)' }; }

  private start() {
    this.defaultPageCounter = 0;
    this.filterer.start(200);
    this.paginator.start(1, this.pageSize,
      combineFilters(this.filterer.filterChanged$, this.slice$)
        .map(x => ({ model: x.right, customFilters: x.left} as ServerNeededParams)),
      (pageNumber, serverPageSize, otherParams) => {
        let ns = this.hotLinkInfo.namespace;
        if (!ns && otherParams.model.selector && otherParams.model.selector !== '') { 
          ns = otherParams.model.items[otherParams.model.selector].namespace;
        }
        this.currentHotLinkNamespace = ns;
        let p: URLSearchParams = new URLSearchParams();
        p.set('filter', JSON.stringify(otherParams.model.value));
        p.set('customFilters', JSON.stringify(otherParams.customFilters));
        p.set('disabled', '0');
        p.set('page', JSON.stringify(pageNumber + 1));
        p.set('per_page', JSON.stringify(serverPageSize));
        return this.httpService.getHotlinkData(ns, this.selectionType,  p);
      },
    {model: {value: this.modelComponent.model.value}, customFilters: ''});

    this.subscription = this.paginator.clientData.subscribe((d) => {
        this.selectionColumn = d.key;
        if (d.columns) {
          this.columns = d.columns;
        }
        this.gridView$.next({data: d.rows, total: d.total, columns: d.columns });
        setTimeout(() => {
          if (this.tablePopupRef) {
            this.tablePopupRef.popupElement
            let filters = this.tablePopupRef.popupElement.querySelectorAll('[kendofilterinput]');
            if (filters && filters[this.changedFilterIndex]) {
              (filters[this.changedFilterIndex] as HTMLElement).focus();
            }
          }
        }, 100);
    });
    this.filterer.filterChanged$.subscribe(x => {
      this.gridView$.next({data: [], total: 0, columns: this.columns });
      this._gridStyle$.next(this._defaultGridStyle);
    });

    this.filterer.filterChanging$.subscribe(x => {
      this._gridStyle$.next(this._filterTypingGridStyle);
    });
  }

  private stop() {
    this.paginator.stop();
    this.filterer.stop();
  }

  public onFilterChange(filter: CompositeFilterDescriptor): void {
    this.filter = filter;
  }

  public onComboFilterChange(filter: string): void {
    this.filter = {logic: 'and', filters: [{field: this.selectionColumn, operator: 'contains', value: filter}]};
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

  private loadTable() { this.start(); }

  selectionTypeChanged(type: string) {
    this.selectionType = type;
    this.closeOptions();
  }

  selectionChanged(value: any) {
    let idx = this.paginator.getClientPageIndex(value.index);
    let k = this.gridView$.value.data.slice(idx, idx + 1);
    this.value = k[0][this.selectionColumn];
    if (this.modelComponent && this.modelComponent.model) {
      this.modelComponent.model.value = this.value;
    }
    this.emitModelChange();
  }

  emitModelChange() {
    // setTimeout to avoid ExpressionChangedAfterItHasBeenCheckedError
    setTimeout(() => this.eventDataService.change.emit(this.modelComponent.cmpId));
  }

  private loadOptions() {
    let ns = this.currentHotLinkNamespace;
    if (!ns) ns = this.hotLinkInfo.namespace;
    this.httpService.getHotlinkSelectionTypes(ns).toPromise().then(json => this.selectionTypes = json.selections);
  }

  ngOnInit() {
    // fix for themes css conflict in form.scss style 
    if(this.modelComponent && !this.modelComponent.width) {
      let textBox = (this.vcr.element.nativeElement.parentNode.getElementsByClassName('k-textbox') as HTMLCollection).item(0);
      if (textBox) { (textBox as HTMLElement).style.width = 'auto'; }
    }
  }

  ngOnDestroy() {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
    this.closePopups();
    if (this.tableSub) {
      this.tableSub.unsubscribe();
    }
    if (this.optionsSub) {
      this.optionsSub.unsubscribe();
    }
  }
}
