import { Injectable, ViewChild } from '@angular/core';
import { Subject } from 'rxjs/Subject';

@Injectable()
export class SidenavService {

  private sidenavOpenedSource = new Subject<boolean>();
  sidenavOpened$ = this.sidenavOpenedSource.asObservable();

  openSidenav() {
    this.sidenavOpenedSource.next(true);
  }

  closeSidenav() {
    this.sidenavOpenedSource.next(false);
  }

  toggleSidenav() {
    if (this.sidenavOpenedSource) {
      this.closeSidenav();
    } else {
      this.openSidenav();
    }
  }
}