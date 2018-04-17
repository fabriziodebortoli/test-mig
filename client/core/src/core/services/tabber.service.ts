/**
 * La sidenav e' comandata da HomeComponent, il quale risulta sottoscritto all'observable sidenavOpened$
 * Ad ogni component che vuole aprire/chiudere la sidenav basta richiamare il metodo toggleSidenav di questo service 
 * (es: home-sidenav.component)
 */
import { Injectable } from '@angular/core';
import { Observable, Subject } from '../../rxjs.imports';

@Injectable()
export class TabberService {

  public currentIndex: number;
  public tabSelectedSource = new Subject<number>();
  public tabMenuSelectedSource = new Subject<number>();
  tabSelected$: Observable<number> = this.tabSelectedSource.asObservable();
  tabMenuSelected$: Observable<any> = this.tabMenuSelectedSource.asObservable();

  selectTab(index: number) {
    this.currentIndex = index;
    this.tabSelectedSource.next(index);
  }

  selectMenuTab() {
    this.tabMenuSelectedSource.next();
  }
}