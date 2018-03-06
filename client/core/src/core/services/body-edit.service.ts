import { TbComponentService } from './../../core/services/tbcomponent.service';
import { HttpService } from './../../core/services//http.service';
import { DocumentService } from './../../core/services/document.service';

import { Injectable } from '@angular/core';
import { Observable, Subject } from '../../rxjs.imports';


@Injectable()
export class BodyEditService {

  public bodyEditName: string;
  public prototype: any;
  public _currentRow: any;
  public currentRowIdx: any;
  public currentGridIdx: number = -1;
  public currentDbtRowIdx: number = -1;
  public model: any;

  public currentPage: number = 0;

  public rowViewVisible: boolean = false;

  public rowHeight: number = 25;
  private minimumRowHeight: number = 25;
  private rowHeightStep: number = 10;

  public skip = -1;

  constructor(
    public httpService: HttpService,
    public tbComponentService: TbComponentService
  ) {
  }

  get currentRow(): boolean {
    return this._currentRow;
  }

  set currentRow(val: boolean) {
    this._currentRow = val;

    for (var prop in this._currentRow) {
      this._currentRow[prop].enabled = this.model.prototype[prop].enabled;
    }
  }


  increaseRowHeight() {
    this.rowHeight += 10;
  }

  decreaseRowHeight() {
    if (this.rowHeight > this.minimumRowHeight + 10)
      this.rowHeight -= 10;
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
    this.currentDbtRowIdx++;
    this.currentGridIdx++;
    this.changeRow(this.currentGridIdx);
  }

  changeRow(index: number) {

    let dataItem = this.model.rows[index]

    this.currentRow = dataItem;
    this.currentDbtRowIdx = index + this.skip;
    this.currentGridIdx = index

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