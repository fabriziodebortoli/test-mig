import { Response } from '@angular/http';
import { TbComponentService } from './../../core/services/tbcomponent.service';
import { HttpService } from './../../core/services//http.service';
import { DocumentService } from './../../core/services/document.service';

import { Injectable } from '@angular/core';
import { Observable, Subject } from '../../rxjs.imports';

@Injectable()
export class BodyEditService {

  public enabled: boolean = false;
  public isLoading: boolean = false;
  public bodyEditName: string;
  public prototype: any;
  public _currentRow: any;
  public rowCount: number = 0;
  public rows = [];

  public currentRowIdx: any;
  public currentGridIdx: number = -1;
  public currentDbtRowIdx: number = -1;

  public currentPage: number = 0;

  public rowViewVisible: boolean = false;

  public pageSize: number = 15;
  public rowHeight: number = 22;
  private minimumRowHeight: number = 22;
  private rowHeightStep: number = 10;

  public lastEditedRowIndex: number = -1;
  public lastEditedColumnIndex: number = -1;


  public skip = -1;

  constructor(
    public httpService: HttpService,
    public tbComponentService: TbComponentService
  ) {
  }

  get currentRow(): any {
    return this._currentRow;
  }

  set currentRow(currentRow: any) {
    if (!currentRow || !this.prototype)
      return;

    this._currentRow = currentRow;
    for (var prop in this._currentRow) {
      this._currentRow[prop].enabled = this.prototype[prop].enabled && this.enabled;
    }
  }

  setModel(model: any) {
    this.rows = model.rows;
    this.prototype = model.prototype;
    this.rowCount = model ? model.rowCount : 0;
    this.skip = model && model.start ? model.start : 0;
    this.currentDbtRowIdx = model.currentRowIdx;
    this.currentGridIdx = model.currentRowIdx - this.skip;
    if (model.rows && this.currentGridIdx >= 0 && this.currentGridIdx <= model.rows.length)
      this.currentRow = model.rows[this.currentGridIdx];
  }

  increaseRowHeight() {
    this.rowHeight += this.rowHeightStep;
  }

  decreaseRowHeight() {
    if (this.rowHeight >= this.minimumRowHeight + this.rowHeightStep)
      this.rowHeight -= this.rowHeightStep;
  }

  pageChange(event) {
    this.skip = event.skip;

    this.changeDBTRange(this.skip, this.pageSize, this.skip);
    // let sub = this.changeRow(this.skip).subscribe((res) => {
    //   if (res) {
    //     this.changeDBTRange();
    //   }
    //   sub.unsubscribe();
    // });
  }

  setRowViewVisibility(visible: boolean) {
    this.rowViewVisible = visible;
  }

  //-----------------------------------------------------------------------------------------------
  getColumnLength(colName, title): number {
    let length = (this.prototype && this.prototype[colName] && this.prototype[colName].length > 0)
      ? Math.max(title.length, this.prototype && this.prototype[colName].length)
      : title ? title.length : 10;
    return length;
  }

  //-----------------------------------------------------------------------------------------------
  ben_row_changed(item) {

    let selectedRow = item.selectedRows[0];
    if (!selectedRow || !selectedRow.dataItem)
      return;

    this.changeRow(selectedRow.index);
  }

  firstRow() {
    this.currentDbtRowIdx = 0
    this.currentGridIdx = 0
    this.skip = 0;
    this.changeRow(this.currentDbtRowIdx);
  }

  prevRow() {
    this.currentDbtRowIdx--;
    this.currentGridIdx--;
    if (this.currentDbtRowIdx >= this.skip && this.currentDbtRowIdx < this.skip + this.pageSize - 1) {
      //se sono nel range delle righe che ho già scaricato, non faccio niente, se non allineare tutto il record
      this.changeRow(this.currentDbtRowIdx);
    }
    else {
      //
      this.skip = this.skip + 1 - this.pageSize;
      this.changeDBTRange(this.skip, this.pageSize, this.currentDbtRowIdx);
      return;
    }

  }

  nextRow() {
    this.currentDbtRowIdx++;
    this.currentGridIdx++;

    if (this.currentDbtRowIdx >= this.skip && this.currentDbtRowIdx < this.skip + this.pageSize - 1) {
      //se sono nel range delle righe che ho già scaricato, non faccio niente, se non allineare tutto il record
      this.changeRow(this.currentDbtRowIdx);
    }
    else {
      this.skip = this.skip + this.pageSize - 1;
      this.changeDBTRange(this.skip, this.pageSize, this.currentDbtRowIdx);
    }
  }

  lastRow() {
    //qui devo scatenare il pagechange se ci sono più righe di quante ne sto vedendo
    this.currentDbtRowIdx = this.rowCount - 1;
    this.currentGridIdx = this.rowCount - 1;
    this.changeRow(this.currentGridIdx)
  }

  //nel range di righe selezionate, posiziona sulla riga desiderata e comunica la stessa informazione al server
  changeRow(index: number) {

    let idx = index - this.skip;
    this.currentDbtRowIdx = index;
    this.currentGridIdx = idx;

    let dataItem = this.rows[idx];
    if (!dataItem) {
      return;
    }

    this.currentRow = dataItem;

    let docCmpId = (this.tbComponentService as DocumentService).mainCmpId;
    let sub = this.httpService.changeRowDBTSlaveBuffered(docCmpId, this.bodyEditName, index).subscribe(() => {
      sub.unsubscribe;
    });
  }

  //avvisa il server che è richiesto un nuovo set di righe
  changeDBTRange(skip: number, rowsToTake: number, desiredRow: number) {
    let docCmpId = (this.tbComponentService as DocumentService).mainCmpId;
    this.isLoading = true;
    console.log("skip, rowsToTake, rowSelected", skip, rowsToTake, desiredRow)
    let sub = this.httpService.getDBTSlaveBufferedModel(docCmpId, this.bodyEditName, skip, rowsToTake, desiredRow).subscribe((res) => {
      sub.unsubscribe();
    });
  }
}