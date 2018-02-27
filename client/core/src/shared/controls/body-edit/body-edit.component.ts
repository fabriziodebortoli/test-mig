import { FormMode } from './../../models/form-mode.enum';
import { addModelBehaviour } from './../../../shared/models/control.model';
import { untilDestroy } from './../../commons/untilDestroy';

import { Observable } from 'rxjs/Rx';
import { ControlComponent } from './../control.component';

import { Store } from './../../../core/services/store.service';
import { createSelectorByMap } from './../../commons/selector';
import { BodyEditColumnComponent } from './../body-edit-column/body-edit-column.component';
import { TbComponentService } from './../../../core/services/tbcomponent.service';
import { LayoutService } from './../../../core/services/layout.service';
import { HttpService } from './../../../core/services/http.service';
import { DocumentService } from './../../../core/services/document.service';
import { EventDataService } from './../../../core/services/eventdata.service';

import { Component, OnInit, Input, OnDestroy, ContentChildren, HostListener, ChangeDetectorRef, ViewChild, AfterContentInit, AfterViewInit, ChangeDetectionStrategy, Directive, ElementRef, ViewEncapsulation } from '@angular/core';
import { Subscription, BehaviorSubject } from '../../../rxjs.imports';
import { SelectableSettings } from '@progress/kendo-angular-grid/dist/es/selection/selectable-settings';
import { GridDataResult, PageChangeEvent } from '@progress/kendo-angular-grid';
import { GridComponent } from '@progress/kendo-angular-grid';
import { RowArgs } from '@progress/kendo-angular-grid/dist/es/rendering/common/row-class';
import { apply, diff } from 'json8-patch';
import * as _ from 'lodash';

const resolvedPromise = Promise.resolve(null); //fancy setTimeout

@Component({

  selector: 'tb-body-edit',
  templateUrl: './body-edit.component.html',
  styleUrls: ['./body-edit.component.scss']
})
export class BodyEditComponent extends ControlComponent implements AfterContentInit, AfterViewInit, OnDestroy {

  @ContentChildren(BodyEditColumnComponent) be_columns;
  @ViewChild(GridComponent) grid;

  @Input() bodyEditName: string;
  @Input() height: number;
  @Input() columns: Array<any>;
  public selector: any;
  public selectableSettings: SelectableSettings;

  isRowSelected = (e: RowArgs) => e.index == this.currentGridIdx;
  currentGridIdx: number = -1;
  currentDbtRowIdx: number = -1;
  subscriptions = [];
  lastTimeStamp: number;
  subscription = [];

  public skip = 0;
  public currentRow: any = undefined;
  public enabled: boolean = false;
  public isLoading: boolean = false;
  public pageSizes = false;
  public previousNext = true;
  public pageSize: number = 15;
  public rowHeight: number = 25;
  public currentPage: number = 0;
  public rowCount: number = 0;
  public gridView: GridDataResult;

  private lastEditedRowIndex: number = -1;
  private lastEditedColumnIndex: number = -1;
  constructor(
    public cdr: ChangeDetectorRef,
    public layoutService: LayoutService,
    public tbComponentService: TbComponentService,
    public httpService: HttpService,
    public store: Store,
    private eventData: EventDataService
  ) {
    super(layoutService, tbComponentService, cdr);
    this.selectableSettings = { checkboxOnly: false, mode: "single" };
  }

  //-----------------------------------------------------------------------------------------------
  @HostListener('window:keydown', ['$event'])
  public keyup(event: KeyboardEvent): void {
    if (event.shiftKey && event.keyCode == 9) {
      this.editPreviousColumn();
      return;
    }
    if (event.keyCode === 9) {
      this.editNextColumn();
      return;
    }
  }

  //-----------------------------------------------------------------------------------------------
  editNextColumn() {
    if (this.lastEditedRowIndex < 0 || this.lastEditedColumnIndex < 0) {
      return;
    }

    if (this.lastEditedColumnIndex < this.grid.columns.length) {
      this.lastEditedColumnIndex++;
    }
    else {
      this.lastEditedColumnIndex = 0
      this.lastEditedRowIndex++;
    }
    this.grid.editCell(this.lastEditedRowIndex, this.lastEditedColumnIndex);
  }

  //-----------------------------------------------------------------------------------------------
  editPreviousColumn() {
    if (this.lastEditedRowIndex < 0 || this.lastEditedColumnIndex < 0) {
      return;
    }

    if (this.lastEditedColumnIndex > 0) {
      this.lastEditedColumnIndex--;
    }
    else {
      this.lastEditedColumnIndex = this.grid.columns.length;
      this.lastEditedRowIndex--;
    }
    this.grid.editCell(this.lastEditedRowIndex, this.lastEditedColumnIndex);
  }

  //-----------------------------------------------------------------------------------------------
  subscribeToSelector() {
    if (this.store && this.selector) {
      this.store
        .select(this.selector)
        .pipe(untilDestroy(this))
        .subscribe((v) => {
          this.getModelForDbt(v.timeStamp);
        }
        );

      this.store.select(m => _.get(m, 'FormMode.value'))
        .subscribe(m => {
          this.enabled = (m === FormMode.EDIT || m === FormMode.NEW) && (this.model && this.model.enabled);
          this.changeDetectorRef.markForCheck();
        });
    }
  }

  //-----------------------------------------------------------------------------------------------
  ngOnDestroy() {
    //this.paginator.stop();
  }

  //-----------------------------------------------------------------------------------------------
  ngAfterContentInit() {
    resolvedPromise.then(() => {
      let cols = this.be_columns.toArray();
      let internalColumnComponents = [];
      for (let i = 0; i < cols.length; i++) {
        internalColumnComponents.push(cols[i].columnComponent);
      }
      this.grid.columns.reset(internalColumnComponents);
    });

    let numberOfVisibleRows = Math.ceil(this.height / this.rowHeight);
    this.pageSize = Math.max(this.pageSize, numberOfVisibleRows);
    this.selector = createSelectorByMap({ timeStamp: this.bodyEditName + '.timeStamp' });
    this.subscribeToSelector();
  }

  //-----------------------------------------------------------------------------------------------
  ngAfterViewInit() {
    this.changeDetectorRef.markForCheck();
  }

  pageChange(event) {
    this.skip = event.skip;
    console.log("pagechange", event.skip, event.take);
    let docCmpId = (this.tbComponentService as DocumentService).mainCmpId;
    this.httpService.getDBTSlaveBufferedModel(docCmpId, this.bodyEditName, this.skip, this.pageSize).subscribe((data) => {
      let dbt = data[this.bodyEditName];
      this.updateModel(dbt);
    });

  }

  //-----------------------------------------------------------------------------------------------
  public cellClickHandler({ sender, rowIndex, columnIndex, dataItem, isEdited }) {
    if (!isEdited) {

      let colComponent = this.grid.columns.toArray()[columnIndex];
      let colName = colComponent.field;
      let isEnabled = this.currentRow[colName].enabled
      if (isEnabled) {
        this.lastEditedRowIndex = rowIndex;
        this.lastEditedColumnIndex = columnIndex;
        sender.editCell(rowIndex, columnIndex);
      }
    }
  }

  //-----------------------------------------------------------------------------------------------
  public cellCloseHandler(args: any) {
    // this.lastEditedRowIndex = -1;
    // this.lastEditedColumnIndex = -1;
  }

  //-----------------------------------------------------------------------------------------------
  ben_row_changed(item) {

    let selectedRow = item.selectedRows[0];
    if (!selectedRow || !selectedRow.dataItem)
      return;

    this.currentRow = selectedRow.dataItem;
    this.currentGridIdx = selectedRow.index;
    this.currentDbtRowIdx = selectedRow.index + this.skip;

    for (var prop in selectedRow.dataItem) {
      this.currentRow[prop].enabled = this.model.prototype[prop].enabled;
    }

    let docCmpId = (this.tbComponentService as DocumentService).mainCmpId;
    let sub = this.httpService.changeRowDBTSlaveBuffered(docCmpId, this.bodyEditName, selectedRow.index).subscribe((res) => {

      // addModelBehaviour(selectedRow.dataItem);
      // this.currentRowIdx = selectedRow.index;
      // this.currentRow = selectedRow.dataItem;
    });
  }

  addRow() {
    let docCmpId = (this.tbComponentService as DocumentService).mainCmpId;

    let tempPageSize = this.pageSize;
    let tempCount = this.rowCount;
    tempCount++;
    let skip = (Math.ceil(tempCount / this.pageSize) * this.pageSize) - this.pageSize;

    let sub = this.httpService.addRowDBTSlaveBuffered(docCmpId, this.bodyEditName, skip, tempPageSize).subscribe((res) => {

      if (res && res[this.bodyEditName]) {
        this.updateModel(res[this.bodyEditName]);
      }
      else {
        this.pageChange({ skip: skip, take: this.pageSize });
      }

      sub.unsubscribe();
    });
  }

  removeRow() {
    if (this.currentGridIdx < 0)
      return;

    let docCmpId = (this.tbComponentService as DocumentService).mainCmpId;
    let sub = this.httpService.removeRowDBTSlaveBuffered(docCmpId, this.bodyEditName, this.currentDbtRowIdx).subscribe((res) => {
      let dbt = res[this.bodyEditName];
      this.updateModel(dbt);

      sub.unsubscribe();
    });
  }

  changeRow() {
    if (this.currentGridIdx < 0)
      return;

    let docCmpId = (this.tbComponentService as DocumentService).mainCmpId;
    let sub = this.httpService.changeRowDBTSlaveBuffered(docCmpId, this.bodyEditName, this.currentDbtRowIdx).subscribe((res) => {
      this.updateModel(res.dbt);
      sub.unsubscribe();
    });
  }

  openRowView() {

  }

  enableMultiselection() {

  }

  enhancedView() {

  }

  increaseRowHeight() {

  }

  decreaseRowHeight() {

  }

  //-----------------------------------------------------------------------------------------------
  getModelForDbt(timeStamp: any) {
    if (!this.model || !timeStamp)
      return;

    let serverUtc = new Date(timeStamp).getTime();

    if (!this.lastTimeStamp || this.lastTimeStamp <= serverUtc) {
      this.isLoading = true;
      this.lastTimeStamp = serverUtc;
      let docCmpId = (this.tbComponentService as DocumentService).mainCmpId;

      // if (this.gridView) {
      //   this.gridView.data = [];
      //   this.gridView.total = 0;
      // }

      console.log("getModelForDbt", this.skip, this.pageSize, this.bodyEditName);
      let sub = this.httpService.getDBTSlaveBufferedModel(docCmpId, this.bodyEditName, this.skip, this.pageSize).subscribe((res) => {

        // if (res.patch) {
        //   const patched = apply({ [this.bodyEditName]: this.model }, res.patch);
        //   res.data = patched.doc;
        // }
        if (res) {

          console.log("getModelForDbt updatemodel")
          let dbt = res[this.bodyEditName];
          this.updateModel(dbt);
        }
        this.isLoading = false;
        sub.unsubscribe();
      });
    }
  }

  private updateModel(dbt: any) {

    if (!dbt) {
      console.log("not a dbt", dbt);
      return;
    }

    addModelBehaviour(dbt);
    this.model.enabled = dbt.enabled;
    let tempIndex = 0;
    let temp = [];
    this.model.rows = [];
    for (let index = this.skip; index < this.skip + this.pageSize; index++) {
      if (dbt.rows[tempIndex]) {
        temp[tempIndex] = this.model.rows[index] = dbt.rows[tempIndex];
      }
      tempIndex++;
    }
    //this.model.rows = dbt.rows;

    this.model.prototype = dbt.prototype;
    this.currentDbtRowIdx = dbt.currentRowIdx;
    console.log("this.currentDbtRowIdx", this.currentDbtRowIdx, "this.skip", this.skip )
    this.currentGridIdx = dbt.currentRowIdx - this.skip;
    this.model.lastTimeStamp = new Date().getTime();
    this.rowCount = dbt.rowCount;

    this.gridView = {
      data: temp,
      total: this.rowCount
    };

    this.eventData.oldModel = JSON.parse(JSON.stringify(this.eventData.model));
    //this.eventData.change.emit('');

    this.changeDetectorRef.markForCheck();
  }
}