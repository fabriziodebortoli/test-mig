/**
 * La sidenav e' comandata da HomeComponent, il quale risulta sottoscritto all'observable sidenavOpened$
 * Ad ogni component che vuole aprire/chiudere la sidenav basta richiamare il metodo toggleSidenav di questo service 
 * (es: home-sidenav.component)
 */
import { Injectable } from '@angular/core';
import { Observable, Subject } from '../../rxjs.imports';

@Injectable()
export class SidenavService {

  public sidenavLeftOpenedSource = new Subject<boolean>();
  sidenavOpenedLeft$: Observable<boolean> = this.sidenavLeftOpenedSource.asObservable();

  public sidenavRightOpenedSource = new Subject<boolean>();
  sidenavOpenedRight$: Observable<boolean> = this.sidenavRightOpenedSource.asObservable();

  toggleSidenavLeft() {
    this.sidenavLeftOpenedSource.next();
  }

  toggleSidenavRight() {
    this.sidenavRightOpenedSource.next();
  }
}