/**
 * La sidenav e' comandata da HomeComponent, il quale risulta sottoscritto all'observable sidenavOpened$
 * Ad ogni component che vuole aprire/chiudere la sidenav basta richiamare il metodo toggleSidenav di questo service 
 * (es: home-sidenav.component)
 */
import { Injectable } from '@angular/core';
import { Observable, BehaviorSubject, Subject } from '../../rxjs.imports';

@Injectable()
export class SidenavService {

  public sidenavOpenedSource = new Subject<boolean>();
  sidenavOpened$: Observable<boolean> = this.sidenavOpenedSource.asObservable();

  private sidenavPinned: boolean = localStorage.getItem('sidenavPinned') == 'true';
  public sidenavPinnedSource = new BehaviorSubject<boolean>(this.sidenavPinned);
  sidenavPinned$: Observable<boolean> = this.sidenavPinnedSource.asObservable();

  openedChange(opened) {
    localStorage.setItem('sidenavOpened', "" + opened)
    this.sidenavOpenedSource.next(opened);
  }

  pinSidenav() {
    this.sidenavPinned = !this.sidenavPinned;
    localStorage.setItem('sidenavPinned', "" + this.sidenavPinned)
    this.sidenavPinnedSource.next(this.sidenavPinned);
  }
}