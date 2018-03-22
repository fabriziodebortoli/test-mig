import { Injectable } from '@angular/core';
import { Observable, BehaviorSubject } from '../../rxjs.imports';

@Injectable()
export class LayoutService {

  // BehaviorSubject imposta un valore di default e rilascia l'ultimo valore nextato in caso di subscription successiva 
  public viewHeight = new BehaviorSubject<number>(0);
  public widthFactor = new BehaviorSubject<number>(0);
  public heightFactor = new BehaviorSubject<number>(0);



  setViewHeight(viewHeight: number) {
    this.viewHeight.next(viewHeight);
  }

  // viewHeight$ = this.viewHeight.asObservable();
  getViewHeight(): Observable<number> {
    return this.viewHeight;
  }


  setWidthFactor(wf: number) {
    this.widthFactor.next(wf);
  }

  // viewHeight$ = this.viewHeight.asObservable();
  getWidthFactor(): Observable<number> {
    return this.widthFactor;
  }


  setHeightFactor(hf: number) {
    this.heightFactor.next(hf);
  }

  // viewHeight$ = this.viewHeight.asObservable();
  getHeightFactor(): Observable<number> {
    return this.heightFactor;
  }


  detectDPI() {
    let dpiElement = document.getElementById("dpi");
    if (dpiElement) {

      let offsetHeight = dpiElement.offsetHeight;
      let offsetWidth = dpiElement.offsetWidth;
      this.setHeightFactor(offsetHeight / 55);
      this.setWidthFactor(offsetWidth / 55);
      //pixels * dpioffset / 72  = points
    }

  }
}

