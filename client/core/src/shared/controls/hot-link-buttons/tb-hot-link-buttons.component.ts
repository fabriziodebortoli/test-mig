import { TbComponentService } from './../../../core/services/tbcomponent.service';
import { EnumsService } from './../../../core/services/enums.service';
import { EventDataService } from './../../../core/services/eventdata.service';
import { LayoutService } from './../../../core/services/layout.service';
import { ControlComponent } from './../control.component';
import { HttpService } from './../../../core/services/http.service';
import { OnDestroy, OnInit, AfterViewChecked, Component, Input, HostListener, ElementRef,
        ViewChild, AfterViewInit, ChangeDetectionStrategy, ChangeDetectorRef, NgZone, ViewEncapsulation } from '@angular/core';
import { URLSearchParams } from '@angular/http';
import { GridDataResult, PageChangeEvent, PagerComponent,  } from '@progress/kendo-angular-grid';
import { filterBy, FilterDescriptor, CompositeFilterDescriptor } from '@progress/kendo-data-query';
import { PopupService, PopupSettings, PopupRef } from '@progress/kendo-angular-popup';
import { BehaviorSubject, Subscription, Observable } from '../../../rxjs.imports';
import { PaginatorService, ServerNeededParams } from '../../../core/services/paginator.service';
import { FilterService, combineFilters } from '../../../core/services/filter.services';
import * as _ from 'lodash';

export type HlComponent = { model: any, slice$?: any, cmpId: string, isCombo?: boolean };

@Component({
  selector: 'tb-hotlink-buttons',
  templateUrl: './tb-hot-link-buttons.component.html',
  styleUrls: ['./tb-hot-link-buttons.component.scss'],
  providers: [PaginatorService, FilterService],
  encapsulation: ViewEncapsulation.Emulated,
  changeDetection: ChangeDetectionStrategy.Default
})
export class TbHotlinkButtonsComponent extends ControlComponent implements OnDestroy, AfterViewChecked {

  private _modelComponent: HlComponent
  @Input() public get modelComponent(): HlComponent {
    return this._modelComponent;
  }

  public set modelComponent(value: HlComponent) {
    this._modelComponent = value;
    if (value && value.model) { this.model = value.model; }
  }

  @Input() public namespace: string;
  @Input() public name: string;

  private _slice$: Observable<{ model: any, enabled: boolean }> | any;
  public set slice$(value: Observable<{ model: any, enabled: boolean }> | any) {
    this._slice$ = value;
  }
  public get slice$(): Observable<{ model: any, enabled: boolean }> | any {
    return (!this.modelComponent || !this.modelComponent.slice$) ?  this._slice$ : this.modelComponent.slice$;
  }

  private gridView$ = new BehaviorSubject<{data: any[], total: number, columns: any[]}>
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

  _defaultGridStyle = {'background': 'whitesmoke', 'cursor': 'pointer', 'position': 'fixed',
                       'font-size': 'small', 'border': '1px solid rgba(0,0,0,.05)'};
  _filterTypingGridStyle = {'background': 'whitesmoke', 'color': 'darkgrey', 'position': 'fixed',
                            'font-size': 'small', 'border': '1px solid rgba(0,0,0,.05)'}
  _gridStyle$ = new BehaviorSubject<any>(this._defaultGridStyle);
  get gridStyle(): Observable<any> { return this._gridStyle$.asObservable(); }

  private changedFilterIndex = 0;

  private optionsPopupRef: PopupRef;
  optionsSub: Subscription;

  private tablePopupRef: PopupRef;
  tableSub: Subscription;

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
    private tablePopupService: PopupService
  ) {
    super(layoutService, tbComponentService, changeDetectorRef);
  }

  get isAttachedToAComboBox(): boolean {
    if (this.modelComponent && this.modelComponent.isCombo) { return true; }
    return false;
  }

  async toggleOptions(anchor, template) {
    this.closeTable();
    if (!this.optionsSub) {
      this.optionsSub = this.showOptions$.distinctUntilChanged().subscribe(x => {
        if (x) {
          this.optionsPopupRef = this.optionsPopupService.open({ anchor: anchor, content: template });
          return;
        } else {
          if (this.optionsPopupRef) { this.optionsPopupRef.close(); }
            this.optionsPopupRef = null;
        }
      });
    }
    if (this.showOptionsSubj$.value) { this.closeOptions(); } else { this.openOptions(); }
    await this.loadOptions();
  }

  toggleTable(anchor, template) {
    this.closeOptions();
    if (!this.tableSub) {
      this.tableSub = this.showTable$.distinctUntilChanged().subscribe(x => {
        if (x) {
          this.tablePopupRef = this.tablePopupService.open({ anchor: anchor, content: template });
          this.tablePopupRef.popupOpen.asObservable().subscribe(y => this.loadTable());
        } else {
          if (this.tablePopupRef) {
            this.tablePopupRef.close();
            this.tablePopupRef = null;
          }
        }
      });
    }
    if (this.showTableSubj$.value) { this.closeTable(); } else { this.openTable(); }
  }

  async openDropDown() {
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
  get optionsPopupStyle(): any {
    return {'background': 'whitesmoke', 'border': '1px solid rgba(0,0,0,.05)'};
  }

  getDataItem(d: any): any {
    return d;
  }

  private start() {
    this.defaultPageCounter = 0;
    this.filterer.configure(200);
    this.paginator.start(1, this.pageSize,
      combineFilters(this.filterer.filterChanged$, this.slice$)
        .map(x => ({ model: x.right, customFilters: x.left})),
      (pageNumber, serverPageSize, otherParams) => {
        let p: URLSearchParams = new URLSearchParams(this.args);
        p.set('filter', JSON.stringify(otherParams.model.value));
        p.set('customFilters', JSON.stringify(otherParams.customFilters));
        p.set('disabled', '0');
        p.set('page', JSON.stringify(pageNumber + 1));
        p.set('per_page', JSON.stringify(serverPageSize));
        return this.httpService.getHotlinkData(this.namespace, this.selectionType,  p);
      });

    this.subscription = this.paginator.clientData.subscribe((d) => {
        if (d && d.rows && d.rows.length > 0) {
          this.selectionColumn = d.key;
          this.gridView$.next({data: d.rows, total: d.total, columns: d.columns });
          this.columns = d.columns;
          this.openTable();
          this.closeOptions();
        }
        setTimeout(() => {
          if (this.tablePopupRef) {
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
  }

  ngAfterViewChecked() {
      console.log('VIEW CHECKED!');
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

  private loadTable() {
    this.start();
  }

  selectionTypeChanged(type: string) {
    this.selectionType = type;
    this.closeOptions();
  }

  selectionChanged(value: any) {
    let idx = this.paginator.getClientPageIndex(value.index);
    let k = this.gridView$.value.data.slice(idx, idx + 1);
    this.value = k[0][this.selectionColumn];
    if (this.model) {
      this.model.value = this.value;
      this.eventDataService.change.emit(this.cmpId);
    }
    if (this.modelComponent && this.modelComponent.model) {
      this.modelComponent.model.value = this.value;
      this.eventDataService.change.emit(this.modelComponent.cmpId)
    }
  }

  private async loadOptions() {
    if (this.selectionTypes.length === 0) {
      let json = await this.httpService.getHotlinkSelectionTypes(this.namespace).toPromise();
      this.selectionTypes = json.selections;
      return;
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
