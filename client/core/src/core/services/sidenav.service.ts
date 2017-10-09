/**
 * La sidenav e' comandata da HomeComponent, il quale risulta sottoscritto all'observable sidenavOpened$
 * Ad ogni component che vuole aprire/chiudere la sidenav basta richiamare il metodo toggleSidenav di questo service 
 * (es: home-sidenav.component)
 */
import { Injectable } from '@angular/core';
import { Subject } from 'rxjs/Subject';
import { Observable } from 'rxjs/Rx';

@Injectable()
export class SidenavService {

  public sidenavOpenedSource = new Subject<boolean>();
  sidenavOpened$: Observable<boolean> = this.sidenavOpenedSource.asObservable();

  toggleSidenav() {
    
    this.sidenavOpenedSource.next();
  }
}