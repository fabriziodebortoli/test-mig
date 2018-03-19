import { Observable } from 'rxjs/Rx';
import { Component, OnInit, Input, OnDestroy, QueryList, ContentChildren, HostListener, ChangeDetectorRef, ViewChild, AfterContentInit, AfterViewInit, ChangeDetectionStrategy, Directive, ElementRef, ViewEncapsulation } from '@angular/core';
import { Subscription, BehaviorSubject } from '../../../rxjs.imports';
import { SelectableSettings } from '@progress/kendo-angular-grid/dist/es/selection/selectable-settings';
import { GridDataResult, PageChangeEvent } from '@progress/kendo-angular-grid';
import { GridComponent } from '@progress/kendo-angular-grid';
import { RowArgs } from '@progress/kendo-angular-grid/dist/es/rendering/common/row-class';
import * as _ from 'lodash';

import { FormMode } from './../../models/form-mode.enum';
import { addModelBehaviour } from './../../../shared/models/control.model';
import { untilDestroy } from './../../commons/untilDestroy';
import { ControlComponent } from './../control.component';
import { Store } from './../../../core/services/store.service';
import { createSelectorByMap } from './../../commons/selector';
import { BodyEditColumnComponent } from './body-edit-column/body-edit-column.component';
import { TbComponentService } from './../../../core/services/tbcomponent.service';
import { LayoutService } from './../../../core/services/layout.service';
import { HttpService } from './../../../core/services/http.service';
import { DocumentService } from './../../../core/services/document.service';
import { EventDataService } from './../../../core/services/eventdata.service';
import { BodyEditService } from './../../../core/services/body-edit.service';

const resolvedPromise = Promise.resolve(null); //fancy setTimeout

@Component({
  selector: 'tb-body-edit',
  templateUrl: './body-edit.component.html',
  styleUrls: ['./body-edit.component.scss'],
  providers: [BodyEditService]
})
export class BodyEditComponent extends ControlComponent implements AfterContentInit, AfterViewInit, OnDestroy {

  @ContentChildren(BodyEditColumnComponent) be_columns: QueryList<BodyEditColumnComponent>;
  @ViewChild(GridComponent) grid;

  @Input() bodyEditName: string;
  @Input() height: number;
  @Input() columns: Array<any>;
  public selectableSettings: SelectableSettings;

  isRowSelected = (e: RowArgs) => e.index == this.bodyEditService.currentDbtRowIdx;
  fakeRows = [];
  subscriptions = [];

  private numberOfColumns: number = 0;

  public enabled: boolean = false;

  public pageSizes = false;
  public previousNext = true;

  public gridView: GridDataResult = { data: [], total: 0 };

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
  subscribeToSelector() {
    this.store.select('FormMode.value')
      .subscribe(m => {
        this.enabled = (m === FormMode.EDIT || m === FormMode.NEW) && (this.model && this.model.enabled);
        this.changeDetectorRef.markForCheck();
      });

    let name = this.bodyEditName;
    this.store.select(name)
      .subscribe(m => {
        if (!m)
          return;

        //todoluca, devo solo verificare di non fare subscribe multiple
        if (m.modelChanged) {
          m.modelChanged
            .pipe(untilDestroy(this))
            .subscribe(() => {
              this.updateModel(this.model);
              this.bodyEditService.isLoading = false;
            });
        }
      });
  }

  //-----------------------------------------------------------------------------------------------
  ngOnDestroy() {
  }

  //-----------------------------------------------------------------------------------------------
  ngAfterContentInit() {
    this.bodyEditService.bodyEditName = this.bodyEditName;
    let numberOfVisibleRows = Math.ceil(this.height / this.bodyEditService.rowHeight);
    this.createFakeRows(numberOfVisibleRows);
    this.bodyEditService.pageSize = Math.max(this.bodyEditService.pageSize, numberOfVisibleRows);

    this.subscriptions.push(this.be_columns.changes.subscribe((changes) => {
      if (this.numberOfColumns != changes.length) {
        this.numberOfColumns = changes.length;
        this.resetBodyEditColumns();
      }
    }));

    if (this.bodyEditService.skip < 0) {
      this.bodyEditService.skip = 0;
      this.bodyEditService.changeDBTRange();
      this.bodyEditService.isLoading = false;
    }

    resolvedPromise.then(() => {
      this.resetBodyEditColumns();
    });

    this.subscribeToSelector();
  }

  resetBodyEditColumns() {
    resolvedPromise.then(() => {
      let cols = this.be_columns.toArray();
      let internalColumnComponents = [];
      for (let i = 0; i < cols.length; i++) {
        internalColumnComponents.push(cols[i].columnComponent);
      }
      this.grid.columns.reset(internalColumnComponents);
    });
  }


  //-----------------------------------------------------------------------------------------------
  ngAfterViewInit() {
    this.changeDetectorRef.markForCheck();
  }

  //-----------------------------------------------------------------------------------------------
  public cellClickHandler({ sender, rowIndex, columnIndex, dataItem, isEdited }) {
    if (!isEdited) {

      let colComponent = this.grid.columns.toArray()[columnIndex];
      let colName = colComponent.field;
      let isEnabled = this.bodyEditService.currentRow[colName].enabled && this.enabled
      if (isEnabled) {
        this.lastEditedRowIndex = rowIndex;
        this.lastEditedColumnIndex = columnIndex;
        sender.editCell(rowIndex, columnIndex);
      }
    }
  }

  //-----------------------------------------------------------------------------------------------
  createFakeRows(numberOfVisibleRows: number) {
    for (let index = 0; index < numberOfVisibleRows; index++) {
      this.fakeRows.push({})
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

    this.bodyEditService.changeRow(selectedRow.index);
  }

  //-----------------------------------------------------------------------------------------------
  addRow() {
    let docCmpId = (this.tbComponentService as DocumentService).mainCmpId;

    let tempCount = this.bodyEditService.rowCount + 1;
    let skip = (Math.ceil(tempCount / this.bodyEditService.pageSize) * this.bodyEditService.pageSize) - this.bodyEditService.pageSize;

    let sub = this.httpService.addRowDBTSlaveBuffered(docCmpId, this.bodyEditName, skip, this.bodyEditService.pageSize, tempCount).subscribe((res) => {

      if (res && res[this.bodyEditName]) {
        this.updateModel(res[this.bodyEditName]);
      }
      else {
        this.bodyEditService.pageChange({ skip: skip, take: this.bodyEditService.pageSize });
      }

      sub.unsubscribe();
    });
  }

  //-----------------------------------------------------------------------------------------------
  removeRow() {
    if (this.bodyEditService.currentGridIdx < 0)
      return;

    let docCmpId = (this.tbComponentService as DocumentService).mainCmpId;
    let sub = this.httpService.removeRowDBTSlaveBuffered(docCmpId, this.bodyEditName, this.bodyEditService.currentDbtRowIdx).subscribe((res) => {
      let dbt = res[this.bodyEditName];
      this.updateModel(dbt);

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
  private updateModel(model: any) {

    if (!model) {
      return;
    }

    this.bodyEditService.setModel(model);

    this.gridView = {
      data: this.bodyEditService.rows,
      total: this.bodyEditService.rowCount
    };

    this.changeDetectorRef.markForCheck();
  }
}