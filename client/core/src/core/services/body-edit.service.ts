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
  public currentRowIdx: any;
  public currentGridIdx: number = -1;
  public currentDbtRowIdx: number = -1;
  public model: any;

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
    this._currentRow = currentRow;

    for (var prop in this._currentRow) {
      this._currentRow[prop].enabled = this.model.prototype[prop].enabled;
    }
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

  increaseRowHeight() {
    this.rowHeight += this.rowHeightStep;
  }

  decreaseRowHeight() {
    if (this.rowHeight >= this.minimumRowHeight + this.rowHeightStep)
      this.rowHeight -= this.rowHeightStep;
  }

  setRowViewVisibility(visible: boolean) {
    this.rowViewVisible = visible;
  }

  prevRow() {
    this.currentDbtRowIdx--;
    this.currentGridIdx--;
    this.changeRow(this.currentGridIdx);

  }
  nextRow() {
    //this.pageChange({ skip: skip, take: this.bodyEditService.pageSize });
    this.currentDbtRowIdx++;
    this.currentGridIdx++;
    this.changeRow(this.currentGridIdx);
  }

  changeRow(index: number) {
    let dataItem = this.model.rows[index];
    if (!dataItem){
      return;
    }

    this.currentRow = dataItem;
    this.currentDbtRowIdx = index + this.skip;
    this.currentGridIdx = index;

    let docCmpId = (this.tbComponentService as DocumentService).mainCmpId;
    let sub = this.httpService.changeRowDBTSlaveBuffered(docCmpId, this.bodyEditName, index).subscribe((res) => {
      sub.unsubscribe();
    });
  }

  firstRow() {
    //qui devo scatenare il pagechange se ci sono più righe di quante ne sto vedendo
    this.currentDbtRowIdx = 0
    this.currentGridIdx = 0
    this.changeRow(this.currentGridIdx);
  }

  lastRow() {
    //qui devo scatenare il pagechange se ci sono più righe di quante ne sto vedendo
    this.currentDbtRowIdx = this.model.rowCount;
    this.currentGridIdx = this.model.rowCount;
    this.changeRow(this.currentGridIdx);
  }
}