import { Injectable } from '@angular/core';
import { Observable } from 'rxjs/Rx';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';

@Injectable()
export class LayoutService {

  // BehaviorSubject imposta un valore di default e rilascia l'ultimo valore nextato in caso di subscription successiva 
  private viewHeight = new BehaviorSubject<number>(0);
  private widthFactor = new BehaviorSubject<number>(0);
  private heightFactor = new BehaviorSubject<number>(0);



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

      console.log("width: " + offsetWidth + " height: " + offsetHeight)

      this.setHeightFactor(offsetHeight / 72);
      this.setWidthFactor(offsetWidth / 72);
      //pixels * dpioffset / 72  = points
    }

  }
}

