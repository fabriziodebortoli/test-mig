import { TbComponentService } from './../../../core/services/tbcomponent.service';
import { EnumsService } from './../../../core/services/enums.service';
import { EventDataService } from './../../../core/services/eventdata.service';
import { LayoutService } from './../../../core/services/layout.service';
import { ControlComponent } from './../control.component';
import { HttpService } from './../../../core/services/http.service';
import { OnDestroy, Component, Input, HostListener, ElementRef,
        ViewChild, ChangeDetectionStrategy, ChangeDetectorRef, NgZone } from '@angular/core';
import { URLSearchParams } from '@angular/http';
import { GridDataResult, PageChangeEvent } from '@progress/kendo-angular-grid';
import { BehaviorSubject, Subscription } from '../../../rxjs.imports';
import { PaginatorService } from '../../../core/services/paginator.service';

@Component({
  selector: 'tb-hotlink-buttons',
  templateUrl: './tb-hot-link-buttons.component.html',
  styleUrls: ['./tb-hot-link-buttons.component.scss'],
  providers: [{provide: PaginatorService, useClass: PaginatorService}],
  changeDetection: ChangeDetectionStrategy.Default
})

export class TbHotlinkButtonsComponent extends ControlComponent implements OnDestroy {

  @Input() namespace: string;
  @Input() name: string;
  @ViewChild('anchor') public anchor: ElementRef;
  @ViewChild('popup', { read: ElementRef }) public popup: ElementRef;
  private gridView = new BehaviorSubject<{data: any[], total: number, columns: any[]}>
  ({data: [], total: 0, columns: [] });
  public columns: any[];
  public selectionTypes: any[] = [];
  public selectionType = 'byOrderDate';

  private buttonCount =  5;
  private info = true;
  private type: 'numeric' | 'input' = 'numeric';
  private pageSizes = false;
  private previousNext = true;
  private pageSize = 10;
  private _prevSkip = -1;
  private _skip = 0;
  private get skip(): number {
    return this._skip;
  }
  private set skip(value: number) {
    this._prevSkip = this._skip;
    this._skip = value;
  }

  private get pageDirection(): number{
    if (this._prevSkip === -1) {return this.pageSize; }
    return this._skip - this._prevSkip;
  }

  showTable = new BehaviorSubject(false);
  showOptions = new BehaviorSubject(false);
  selectionColumn = '';

  subscription: Subscription;

  constructor(public httpService: HttpService,
    layoutService: LayoutService,
    public enumService: EnumsService,
    tbComponentService: TbComponentService,
    private eventDataService: EventDataService,
    private paginator: PaginatorService,
    private ngZone: NgZone
  ) {
    super(layoutService, tbComponentService);
    this.paginator.configure(this.pageSize * 5, this.pageSize,
      (page, size) => this.ngZone.runOutsideAngular(() => this.httpService.getHotlinkTestData(page, size)));
    this.subscription = this.paginator.clientData.subscribe((d) => {
      if (d && d.rows && d.rows.length > 0) {
        this.gridView.next({data: d.rows, total: d.total, columns: d.columns });
        this.columns = d.columns;
        this.showTable.next(true);
        this.showOptions.next(false);
      }
    });
  }

  protected async pageChange(event: PageChangeEvent) {
    if (this.skip === event.skip) { return ; }
    let steps = this.paginator.getNumberOfClientSteps(this.skip, event.skip);
    this.skip = event.skip;
    if (this.pageDirection >= 0) {
      await this.paginator.nextPage(steps);
    } else {
      await this.paginator.prevPage(steps);
    }
  }

  private closePopups() {
    this.showOptions.next(false);
    this.showTable.next(false);
  }

  public get isDisabled(): boolean {
    if (!this.model) {
      return true;
    }
    return !this.model.enabled;
  }

  private contains(target: any): boolean {
    return (this.anchor ? this.anchor.nativeElement.contains(target) : false) ||
      (this.popup ? this.popup.nativeElement.contains(target) : false);
  }

  async onSearchClick() {
    if (this.showTable.value) {
      this.showTable.next(false);
      return;
    }

    if (this.pageDirection >= 0) {
      await this.paginator.nextPage();
    } else {
      await this.paginator.prevPage();
    }
  }

  selectionTypeChanged(type: string) {
    this.selectionType = type;
  }

  selectionChanged(value: any) {
    let k = this.gridView.value.data[value.index];
    this.value = k[this.selectionColumn];
    if (this.model) {
      this.model.value = this.value;
      this.eventDataService.change.emit(this.cmpId);
    }
  }

  onFocus() { this.closePopups(); }

  onOptionsClick() {
    this.showTable.next(false);
    if (this.selectionTypes.length === 0) {
      this.httpService.getHotlinkSelectionTypes(this.namespace)
      .subscribe((json) => { this.selectionTypes = json.selections; });
      this.showOptions.next(true);
      return;
    }
    this.showOptions.next(!this.showOptions.value);
  }

  onRowChecked(event, dataItem) {
    if (dataItem.Selected === undefined) {
      dataItem.Selected = false;
    }

    dataItem.Selected = !dataItem.Selected;

    this.value = '';
    if (this.model) {
      this.model.value = this.value;
    }
  }

  getValue(dataItem: string, column) {
    if (column.type === 'Enum') {
      let res = this.enumService.getEnumsItem(dataItem);
      if (res) {
        return res.name;
      }
      return dataItem;
    } else if (column.Type === 'Boolean') {
      return dataItem ? 'Yes' : 'No';
    }
    return dataItem;
  }

  popupStyle() {
    return {
      'max-width': '50%',
      'font-size': 'small'
    };
  }

  inputStyle() {
    return {
      'width': '60%',
    };
  }

  ngOnDestroy() {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }
}
