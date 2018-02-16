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
import { PaginatorService, ServerNeededParams } from './../../../core/services/paginator.service';

import { Component, OnInit, Input, OnDestroy, ContentChildren, ChangeDetectorRef, ViewChild, AfterContentInit, AfterViewInit, ChangeDetectionStrategy, Directive, ElementRef, ViewEncapsulation } from '@angular/core';
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
  styleUrls: ['./body-edit.component.scss'],
  providers: [PaginatorService]
})
export class BodyEditComponent extends ControlComponent implements AfterContentInit, AfterViewInit, OnDestroy {

  @ContentChildren(BodyEditColumnComponent) be_columns;
  @ViewChild(GridComponent) grid;

  @Input() bodyEditName: string;
  @Input() height: number;
  @Input() columns: Array<any>;
  public selector: any;
  public selectableSettings: SelectableSettings;

  isRowSelected = (e: RowArgs) => e.index == this.currentRowIdx;
  currentRowIdx: number = -1;
  subscriptions = [];
  lastTimeStamp: number;
  subscription = [];

  public skip = 0;
  public currentRow: any = undefined;
  public enabled: boolean = false;
  public isLoading: boolean = false;
  public pageSizes = false;
  public previousNext = true;
  public pageSize: number = 4;
  public currentPage: number = 0;
  public rowCount: number = 0;
  public totalPages: number = 0;
  public gridView: GridDataResult;

  constructor(
    public cdr: ChangeDetectorRef,
    public layoutService: LayoutService,
    public tbComponentService: TbComponentService,
    public httpService: HttpService,
    public store: Store,
    private eventData: EventDataService,
    private paginator: PaginatorService,
  ) {
    super(layoutService, tbComponentService, cdr);
    this.selectableSettings = { checkboxOnly: false, mode: "single" };
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

  ngOnDestroy() {
    this.paginator.stop();
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

    this.selector = createSelectorByMap({ timeStamp: this.bodyEditName + '.timeStamp' });
    this.subscribeToSelector();

    this.paginator.start(1, this.pageSize,
      Observable.never<ServerNeededParams>(),
      (pageNumber, serverPageSize, data?) => {
        let docCmpId = (this.tbComponentService as DocumentService).mainCmpId;
        return this.httpService.getDBTSlaveBufferedModel(docCmpId, this.bodyEditName, (pageNumber) * this.pageSize, this.pageSize)
      });

    this.paginator.serverData$.subscribe(data => {
      if (data) {
        let dbt = data[this.bodyEditName];
        this.updateModel(dbt);
      }
    });
  }

  //-----------------------------------------------------------------------------------------------
  ngAfterViewInit() {
    this.changeDetectorRef.markForCheck();
  }

  pageChange(event) {
    this.skip = event.skip;
    this.paginator.pageChange(event.skip, event.take);
  }
  //-----------------------------------------------------------------------------------------------
  public cellClickHandler({ sender, rowIndex, columnIndex, dataItem, isEdited }) {
    if (!isEdited) {
      let columns = Object.getOwnPropertyNames(dataItem);
      let colName = columns[columnIndex];
      if (dataItem[colName].enabled)
        sender.editCell(rowIndex, columnIndex);
    }
  }

  //-----------------------------------------------------------------------------------------------
  public cellCloseHandler(args: any) {
  }

  //-----------------------------------------------------------------------------------------------
  ben_row_changed(item) {

    let selectedRow = item.selectedRows[0];

    let docCmpId = (this.tbComponentService as DocumentService).mainCmpId;
    let sub = this.httpService.changeRowDBTSlaveBuffered(docCmpId, this.bodyEditName, selectedRow.index).subscribe((res) => {
      addModelBehaviour(selectedRow.dataItem);
      this.currentRowIdx = selectedRow.index;
      this.currentRow = selectedRow.dataItem;
      for (var prop in this.currentRow) {
        this.currentRow[prop].enabled = this.model.prototype[prop].enabled;
      }
    });
  }

  addRow() {
    let docCmpId = (this.tbComponentService as DocumentService).mainCmpId;
    let sub = this.httpService.addRowDBTSlaveBuffered(docCmpId, this.bodyEditName).subscribe((res) => {
      this.updateModel(res.dbt);
      sub.unsubscribe();
    });
  }

  removeRow() {
    if (this.currentRowIdx < 0)
      return;

    let docCmpId = (this.tbComponentService as DocumentService).mainCmpId;
    let sub = this.httpService.removeRowDBTSlaveBuffered(docCmpId, this.bodyEditName, this.currentRowIdx).subscribe((res) => {
      this.updateModel(res.dbt);
      sub.unsubscribe();
    });
  }

  changeRow() {
    if (this.currentRowIdx < 0)
      return;

    let docCmpId = (this.tbComponentService as DocumentService).mainCmpId;
    let sub = this.httpService.changeRowDBTSlaveBuffered(docCmpId, this.bodyEditName, this.currentRowIdx).subscribe((res) => {
      this.updateModel(res.dbt);
      sub.unsubscribe();
    });
  }

  //-----------------------------------------------------------------------------------------------
  getModelForDbt(timeStamp: any) {
    if (!this.model || !timeStamp)
      return;

    let serverUtc = new Date(timeStamp).getTime();
    this.currentPage
    this.pageSize

    if (!this.lastTimeStamp || this.lastTimeStamp <= serverUtc) {
      this.isLoading = true;
      this.lastTimeStamp = serverUtc;
      let docCmpId = (this.tbComponentService as DocumentService).mainCmpId;

      if (this.gridView) {
        this.gridView.data = [];
        this.gridView.total = 0;
      }

      let sub = this.httpService.getDBTSlaveBufferedModel(docCmpId, this.bodyEditName, this.skip, this.pageSize).subscribe((res) => {

        // if (res.patch) {
        //   const patched = apply({ [this.bodyEditName]: this.model }, res.patch);
        //   res.data = patched.doc;
        // }
        if (res) {

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


    console.log("model rows", this.model.rows, "dbtrows", dbt.rows);

    //this.skip , this.pageSize
    let tempIndex = 0;
    for (let index = this.skip; index < this.skip + this.pageSize; index++) {
      this.model.rows[index] = dbt.rows[tempIndex];
      tempIndex++;
    }
    console.log("model", this.model);

    //this.model.rows = dbt.rows;

    this.model.prototype = dbt.prototype;
    //this.currentRowIdx = dbt.currentRowIdx;
    this.model.lastTimeStamp = new Date().getTime();
    this.rowCount = dbt.rowCount;
    this.totalPages = Math.ceil(this.rowCount / this.pageSize);
    const temp = this.model.rows.slice(this.skip, this.skip + this.pageSize);
    this.gridView = {
      data: temp,
      total: this.rowCount
    };

    //data: this.items.slice(this.skip, this.skip + this.pageSize),
    this.eventData.oldModel = JSON.parse(JSON.stringify(this.eventData.model));
    //this.eventData.change.emit('');

    this.changeDetectorRef.markForCheck();
  }
}