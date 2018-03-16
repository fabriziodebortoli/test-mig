import { Response } from '@angular/http';
import { TbComponentService } from './../../core/services/tbcomponent.service';
import { HttpService } from './../../core/services//http.service';
import { DocumentService } from './../../core/services/document.service';

import { Injectable } from '@angular/core';
import { Observable, Subject } from '../../rxjs.imports';

@Injectable()
export class BodyEditService {

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
      this._currentRow[prop].enabled = this.prototype[prop].enabled;
    }
  }

  setModel(model: any) {
    this.rows = model.rows;
    this.prototype = model.prototype;
    this.rowCount = model ? model.rowCount : 0;

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

    let sub = this.changeRow(this.skip).subscribe((res) => {
      this.changeDBTRange();
      sub.unsubscribe();
    });
  }

  setRowViewVisibility(visible: boolean) {
    this.rowViewVisible = visible;
  }

  firstRow() {
    this.currentDbtRowIdx = 0
    this.currentGridIdx = 0
    this.changeRow(this.currentDbtRowIdx).subscribe(() => { });
  }

  prevRow() {
    this.currentDbtRowIdx--;
    this.currentGridIdx--;
    if (this.currentDbtRowIdx < this.skip || this.currentDbtRowIdx >= this.skip + this.pageSize) {
      let skip = (Math.ceil(this.currentGridIdx / this.pageSize) * this.pageSize) - this.pageSize;
      this.skip = skip;
      this.pageChange({ skip: this.skip, take: this.pageSize });
      return;
    }
    this.changeRow(this.currentGridIdx);
  }

  nextRow() {
    this.currentDbtRowIdx++;
    this.currentGridIdx++;

    if (this.currentDbtRowIdx < this.skip || this.currentDbtRowIdx >= this.skip + this.pageSize) {
      let skip = (Math.ceil(this.currentGridIdx / this.pageSize) * this.pageSize) - this.pageSize;
      this.skip = skip;
      this.pageChange({ skip: this.skip, take: this.pageSize });
      return;
    }
    this.changeRow(this.currentGridIdx).subscribe(() => { });
  }


  lastRow() {
    //qui devo scatenare il pagechange se ci sono più righe di quante ne sto vedendo
    this.currentDbtRowIdx = this.rowCount - 1;
    this.currentGridIdx = this.rowCount - 1;
    this.changeRow(this.currentGridIdx).subscribe(() => { });
  }

  changeRow(index: number): Observable<Response> {

    let idx = index - this.skip;
    this.currentDbtRowIdx = index;
    this.currentGridIdx = idx;

    let dataItem = this.rows[idx];
    if (!dataItem) {
      return;
    }

    this.currentRow = dataItem;

    let docCmpId = (this.tbComponentService as DocumentService).mainCmpId;
    return this.httpService.changeRowDBTSlaveBuffered(docCmpId, this.bodyEditName, index).map((res: Response) => {
      return res;
    });
  }

  changeDBTRange() {
    let docCmpId = (this.tbComponentService as DocumentService).mainCmpId;
    this.isLoading = true;
    let sub = this.httpService.getDBTSlaveBufferedModel(docCmpId, this.bodyEditName, this.skip, this.pageSize, this.currentDbtRowIdx).subscribe((res) => {
      sub.unsubscribe();
    });
  }

}