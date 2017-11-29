import { TbComponentService } from './../../../core/services/tbcomponent.service';
import { EnumsService } from './../../../core/services/enums.service';
import { EventDataService } from './../../../core/services/eventdata.service';
import { LayoutService } from './../../../core/services/layout.service';
import { ControlComponent } from './../control.component';
import { HttpService } from './../../../core/services/http.service';
import { OnDestroy, OnInit, AfterViewChecked, Component, Input, HostListener, ElementRef,
        ViewChild, AfterViewInit, ChangeDetectionStrategy, ChangeDetectorRef, NgZone, ViewEncapsulation } from '@angular/core';
import { URLSearchParams } from '@angular/http';
import { GridDataResult, PageChangeEvent, PagerComponent } from '@progress/kendo-angular-grid';
import { filterBy, FilterDescriptor, CompositeFilterDescriptor } from '@progress/kendo-data-query';
import { BehaviorSubject, Subscription, Observable } from '../../../rxjs.imports';
import { PaginatorService, ServerNeededParams } from '../../../core/services/paginator.service';
import { FilterService, combineFilters } from '../../../core/services/filter.services';

@Component({
  selector: 'tb-hotlink-buttons',
  templateUrl: './tb-hot-link-buttons.component.html',
  styleUrls: ['./tb-hot-link-buttons.component.scss'],
  providers: [PaginatorService, FilterService],
  encapsulation: ViewEncapsulation.None,
  changeDetection: ChangeDetectionStrategy.Default
})

export class TbHotlinkButtonsComponent extends ControlComponent implements OnDestroy, OnInit, AfterViewChecked, AfterViewInit {

  @Input() namespace: string;
  @Input() name: string;

  @Input() slice$: any;

  @ViewChild('anchor') public anchor: ElementRef;
  @ViewChild('popup', { read: ElementRef }) public popup: ElementRef;
  @ViewChild(PagerComponent) _pagerComponet: PagerComponent;
  private gridView = new BehaviorSubject<{data: any[], total: number, columns: any[]}>
  ({data: [], total: 0, columns: [] });
  public columns: any[];
  public selectionTypes: any[] = [];
  public selectionType = 'code';

  private buttonCount = 2;
  private info = true;
  private type: 'numeric' | 'input' = 'numeric';
  private pageSizes = false;
  private previousNext = false;
  private pageSize = 2;
  private filter: CompositeFilterDescriptor;
  private showTableSubj$ = new BehaviorSubject(false);
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
  loadingData = false;

  constructor(public httpService: HttpService,
    layoutService: LayoutService,
    public enumService: EnumsService,
    tbComponentService: TbComponentService,
    changeDetectorRef: ChangeDetectorRef,
    private eventDataService: EventDataService,
    private paginator: PaginatorService,
    private filterer: FilterService,
    private ngZone: NgZone
  ) {
    super(layoutService, tbComponentService, changeDetectorRef);
  }

  closeOptions() { this.showOptionsSubj$.next(false); }
  openOptions() { this.showOptionsSubj$.next(true); }
  closeTable() { this.showTableSubj$.next(false); }
  openTable() {
    this.showTableSubj$.next(true);
  }
  closePopups() { this.closeOptions(); this.closeTable(); }
  get popupStyle(): any { return {'max-width': '50%', 'font-size': 'small'}; }

  ngOnInit() {
    this.filterer.configure(200);
    this.paginator.configure(this.buttonCount,
      this.pageSize,
      combineFilters(this.filterer.filter$, this.slice$)
      .map((x) => { return { model: x.right, customFilters: x.left}; }),
      (pageNumber, serverPageSize, otherParams?) => {
        let p: URLSearchParams = new URLSearchParams(this.args);
        if (otherParams) {
          p.set('filter', JSON.stringify(otherParams.model));
          p.set('customFilters', JSON.stringify(otherParams.customFilters));
        }

        p.set('disabled', '0');
        p.set('page', JSON.stringify(pageNumber + 1));
        p.set('per_page', JSON.stringify(serverPageSize));
        return this.httpService.getHotlinkData(this.namespace, this.selectionType,  p);
      });
    this.ngZone.runOutsideAngular( () => {
      this.subscription = this.paginator.clientData.subscribe((d) => {
        if (d && d.rows && d.rows.length > 0) {
          this.selectionColumn = d.key;
          this.gridView.next({data: d.rows, total: d.total, columns: d.columns });
          this.columns = d.columns;
          this.openTable();
          this.closeOptions();
        }
        this.loadingData = false;
      });

      this.filterer.filterTyping$.subscribe(x => {
        this.gridView.next({data: [], total: 0, columns: this.columns });
      });
    });
  }

  ngAfterViewInit() {
    console.log(this._pagerComponet);
  }

  ngAfterViewChecked() {
      console.log('VIEW CHECKED!');
  }

  public filterChange(filter: CompositeFilterDescriptor): void {
    this.filterer.filter = filter;
    this.filterer.filterChanged(filter);
  }

  protected async pageChange(event: PageChangeEvent) {
    await this.paginator.pageChange(event.skip, event.take);
  }

  private contains(target: any): boolean {
    return (this.anchor ? this.anchor.nativeElement.contains(target) : false) ||
      (this.popup ? this.popup.nativeElement.contains(target) : false);
  }

  async onSearchClick() {
    if (this.showTableSubj$.value) {this.closeTable(); return; }
    this.loadingData = true;
    await this.paginator.firstPage();
  }

  selectionTypeChanged(type: string) {
    this.selectionType = type;
    this.closeOptions();
  }

  selectionChanged(value: any) {
    let k = this.gridView.value.data[this.paginator.getClientPageIndex(value.index)];
    this.value = k[this.selectionColumn];
    if (this.model) {
      this.model.value = this.value;
      this.eventDataService.change.emit(this.cmpId);
    }
  }

  onOptionsClick() {
    this.closeTable();
    if (this.selectionTypes.length === 0) {
      this.httpService.getHotlinkSelectionTypes(this.namespace)
      .subscribe((json) => { this.selectionTypes = json.selections; });
      this.openOptions();
      return;
    }
    this.showOptionsSubj$.next(!this.showOptionsSubj$.value);
  }

  ngOnDestroy() {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }
}
