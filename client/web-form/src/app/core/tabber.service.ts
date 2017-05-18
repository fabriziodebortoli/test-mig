/**
 * La sidenav e' comandata da HomeComponent, il quale risulta sottoscritto all'observable sidenavOpened$
 * 
 * Ad ogni component che vuole aprire/chiudere la sidenav basta richiamare il metodo toggleSidenav di questo service (es: home-sidenav.component)
 */
import { Injectable } from '@angular/core';
import { Subject } from 'rxjs/Subject';

@Injectable()
export class TabberService {

  private tabSelectedSource = new Subject<number>();
  tabSelected$ = this.tabSelectedSource.asObservable();

  selectTab(index:number) {
    this.tabSelectedSource.next(index);
  }
}