import { Injectable } from '@angular/core';
import { Observable, Subject } from '../../rxjs.imports';

@Injectable()
export class BodyEditService {

  public prototype: any;
  public rowHeight: number = 25;
  minimumRowHeight: number = 25;
  rowHeightStep: number = 10;

  public currentRow: any;
  increaseRowHeight() {

    this.rowHeight += 10;
  }

  decreaseRowHeight() {

    if (this.rowHeight > this.minimumRowHeight + 10)
    this.rowHeight -= 10;
  }
}