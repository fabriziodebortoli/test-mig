import { TbComponentService } from './../../core/services/tbcomponent.service';
import { HttpService } from './../../core/services//http.service';
import { DocumentService } from './../../core/services/document.service';

import { Injectable } from '@angular/core';
import { Observable, Subject } from '../../rxjs.imports';


@Injectable()
export class BodyEditService {

  public bodyEditName: string;
  public prototype: any;
  public currentRow: any;
  public currentRowIdx: any;
  public currentGridIdx: number = -1;
  public currentDbtRowIdx: number = -1;
  public bodyEditModel: any;

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

  firstRow() {
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

    let dataItem = this.bodyEditModel.rows[index]

    this.currentRow = dataItem;
    this.currentDbtRowIdx = index + this.skip;
    this.currentGridIdx = index

    for (var prop in dataItem) {
      this.currentRow[prop].enabled = this.bodyEditModel.prototype[prop].enabled;
    }

    let docCmpId = (this.tbComponentService as DocumentService).mainCmpId;
    let sub = this.httpService.changeRowDBTSlaveBuffered(docCmpId, this.bodyEditName, index).subscribe((res) => {
      sub.unsubscribe();
    });

  }

  lastRow() {
  }
}