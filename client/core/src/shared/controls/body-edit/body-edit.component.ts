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

import { Component, OnInit, Input, OnDestroy, ContentChildren, ChangeDetectorRef, ViewChild, AfterContentInit, AfterViewInit, ChangeDetectionStrategy, Directive, ElementRef, ViewEncapsulation } from '@angular/core';
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
  styleUrls: ['./body-edit.component.scss']
})
export class BodyEditComponent extends ControlComponent implements AfterContentInit, AfterViewInit, OnDestroy {

  @Input() bodyEditName: string;
  @Input() height: number;
  @Input() columns: Array<any>;
  public selector: any;
  public selectableSettings: SelectableSettings;

  currentRowIdx: number = -1;
  subscriptions = [];

  @ContentChildren(BodyEditColumnComponent) be_columns;
  @ViewChild(GridComponent) grid;
  subscription = [];

  public currentRow: any = undefined;
  isRowSelected = (e: RowArgs) => e.index == this.currentRowIdx;
  public enabled: boolean = false;

  constructor(
    public cdr: ChangeDetectorRef,
    public layoutService: LayoutService,
    public tbComponentService: TbComponentService,
    public httpService: HttpService,
    public store: Store,
    private eventData: EventDataService,
  ) {
    super(layoutService, tbComponentService, cdr);

    this.selectableSettings = {
      checkboxOnly: false,
      mode: "single"
    };


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
          this.enabled = (m === FormMode.EDIT || m === FormMode.NEW) &&  (this.model && this.model.enabled);
          this.changeDetectorRef.markForCheck();
        });
    }
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
  }

  //-----------------------------------------------------------------------------------------------
  ngAfterViewInit() {
    this.changeDetectorRef.markForCheck();
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

    if (!this.model.lastTimeStamp || this.model.lastTimeStamp <= serverUtc) {
      this.model.lastTimeStamp = serverUtc;
      let docCmpId = (this.tbComponentService as DocumentService).mainCmpId;
      let sub = this.httpService.getDBTSlaveBufferedModel(docCmpId, this.bodyEditName).subscribe((res) => {

        this.updateModel(res.dbt);
        sub.unsubscribe();
      });
    }
  }

  private updateModel(dbt: any) {

    if (!dbt)
      return;
    addModelBehaviour(dbt);
    this.model.enabled = dbt.enabled;
    this.model.rows = dbt.rows;
    this.model.prototype = dbt.prototype;
    //this.currentRowIdx = dbt.currentRowIdx;
    this.model.lastTimeStamp = new Date().getTime();
    this.changeDetectorRef.markForCheck();
  }
}