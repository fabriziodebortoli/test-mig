import { Injectable } from '@angular/core';
import { Observable } from 'rxjs/Rx';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';

@Injectable()
export class LayoutService {

  // BehaviorSubject imposta un valore di default e rilascia l'ultimo valore nextato in caso di subscription successiva 
  private viewHeight = new BehaviorSubject<number>(0);

  setViewHeight(viewHeight: number) {
    this.viewHeight.next(viewHeight);
  }

  // viewHeight$ = this.viewHeight.asObservable();
  getViewHeight(): Observable<number> {
    return this.viewHeight;
  }

}
