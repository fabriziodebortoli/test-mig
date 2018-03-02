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
import { BodyEditService } from './../../../core/services/body-edit.service';

import { Component, OnInit, Input, OnDestroy, ContentChildren, HostListener, ChangeDetectorRef, ViewChild, AfterContentInit, AfterViewInit, ChangeDetectionStrategy, Directive, ElementRef, ViewEncapsulation } from '@angular/core';
import { Subscription, BehaviorSubject } from '../../../rxjs.imports';
import { SelectableSettings } from '@progress/kendo-angular-grid/dist/es/selection/selectable-settings';
import { GridDataResult, PageChangeEvent } from '@progress/kendo-angular-grid';
import { GridComponent } from '@progress/kendo-angular-grid';
import { RowArgs } from '@progress/kendo-angular-grid/dist/es/rendering/common/row-class';
import * as _ from 'lodash';

const resolvedPromise = Promise.resolve(null); //fancy setTimeout

@Component({

  selector: 'tb-body-edit',
  templateUrl: './body-edit.component.html',
  styleUrls: ['./body-edit.component.scss'],
  providers: [BodyEditService]
})
export class BodyEditComponent extends ControlComponent implements AfterContentInit, AfterViewInit, OnDestroy {

  @ContentChildren(BodyEditColumnComponent) be_columns;
  @ViewChild(GridComponent) grid;

  @Input() bodyEditName: string;
  @Input() height: number;
  @Input() columns: Array<any>;
  public selectableSettings: SelectableSettings;

  
  isRowSelected = (e: RowArgs) => e.index == this.currentGridIdx;
  currentGridIdx: number = -1;
  currentDbtRowIdx: number = -1;
  subscriptions = [];
  subscription = [];

  public skip = -1;
  public pageSize: number = 15;
  
  public enabled: boolean = false;
  public isLoading: boolean = false;
  public pageSizes = false;
  public previousNext = true;
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
    private eventData: EventDataService,
    public bodyEditService: BodyEditService
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
    this.model.modelChanged
      .pipe(untilDestroy(this))
      .subscribe(() => {
        this.updateModel(this.model);
        this.isLoading = false;
      });

    this.store.select('FormMode.value')
      .subscribe(m => {
        this.enabled = (m === FormMode.EDIT || m === FormMode.NEW) && (this.model && this.model.enabled);
        this.changeDetectorRef.markForCheck();
      });
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

    let numberOfVisibleRows = Math.ceil(this.height / this.bodyEditService.rowHeight);
    this.pageSize = Math.max(this.pageSize, numberOfVisibleRows);
    this.subscribeToSelector();

    if (this.skip < 0) {
      this.skip = 0;
      this.changeDBTRange();
    }

    this.bodyEditService.bodyEditName = this.bodyEditName;
  }

  //-----------------------------------------------------------------------------------------------
  ngAfterViewInit() {
    this.changeDetectorRef.markForCheck();
  }

  pageChange(event) {
    this.skip = event.skip;
    this.changeDBTRange();

  }
  changeDBTRange() {
    let docCmpId = (this.tbComponentService as DocumentService).mainCmpId;
    this.isLoading = true;
    let sub = this.httpService.getDBTSlaveBufferedModel(docCmpId, this.bodyEditName, this.skip, this.pageSize).subscribe((res) => {

      sub.unsubscribe();
    });

  }
  //-----------------------------------------------------------------------------------------------
  public cellClickHandler({ sender, rowIndex, columnIndex, dataItem, isEdited }) {
    if (!isEdited) {

      let colComponent = this.grid.columns.toArray()[columnIndex];
      let colName = colComponent.field;
      let isEnabled = this.bodyEditService.currentRow[colName].enabled
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

    console.log("selectedRow this.currentDbtRowIdx", selectedRow, this.currentDbtRowIdx, "this.skip", this.skip)


    this.bodyEditService.currentRow = selectedRow.dataItem;
    this.currentGridIdx = selectedRow.index;
    this.currentDbtRowIdx = selectedRow.index + this.skip;
    console.log("after selected row  this.currentDbtRowIdx", this.currentGridIdx, this.currentDbtRowIdx)
    for (var prop in selectedRow.dataItem) {
      this.bodyEditService.currentRow[prop].enabled = this.model.prototype[prop].enabled;
    }

    let docCmpId = (this.tbComponentService as DocumentService).mainCmpId;
    let sub = this.httpService.changeRowDBTSlaveBuffered(docCmpId, this.bodyEditName, selectedRow.index).subscribe((res) => {
      sub.unsubscribe();
      // addModelBehaviour(selectedRow.dataItem);
      // this.currentRowIdx = selectedRow.index;
      // this.currentRow = selectedRow.dataItem;
    });
  }

  //-----------------------------------------------------------------------------------------------
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

  //-----------------------------------------------------------------------------------------------
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

  //-----------------------------------------------------------------------------------------------
  changeRow() {
    if (this.currentGridIdx < 0)
      return;

    let docCmpId = (this.tbComponentService as DocumentService).mainCmpId;
    let sub = this.httpService.changeRowDBTSlaveBuffered(docCmpId, this.bodyEditName, this.currentDbtRowIdx).subscribe((res) => {
      this.updateModel(res.dbt);
      sub.unsubscribe();
    });
  }

  //-----------------------------------------------------------------------------------------------
  openRowView() {
    this.bodyEditService.setRowViewVisibility(true);
  }

  //-----------------------------------------------------------------------------------------------
  enableMultiselection() {

  }

  //-----------------------------------------------------------------------------------------------
  enhancedView() {

  }

  //-----------------------------------------------------------------------------------------------
  increaseRowHeight() {

    this.bodyEditService.increaseRowHeight();
  }

  //-----------------------------------------------------------------------------------------------
  decreaseRowHeight() {

    this.bodyEditService.decreaseRowHeight();
  }

  //-----------------------------------------------------------------------------------------------
  private updateModel(dbt: any) {

    if (!dbt) {
      console.log("not a dbt", dbt);
      return;
    }

    console.log("updatemodel this.currentDbtRowIdx", this.currentDbtRowIdx, "this.skip", this.skip)

    this.currentDbtRowIdx = dbt.currentRowIdx;
    console.log("this.currentDbtRowIdx", this.currentDbtRowIdx, "this.skip", this.skip)
    this.currentGridIdx = dbt.currentRowIdx - this.skip;
    this.rowCount = dbt.rowCount ? dbt.rowCount : 0;

    this.gridView = {
      data: this.model.rows,
      total: this.rowCount
    };

    this.changeDetectorRef.markForCheck();
  }
}