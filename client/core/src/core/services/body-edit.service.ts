import { Injectable } from '@angular/core';
import { Observable, Subject } from '../../rxjs.imports';

@Injectable()
export class BodyEditService {

  public prototype: any;
  public rowHeight: number = 25;
}