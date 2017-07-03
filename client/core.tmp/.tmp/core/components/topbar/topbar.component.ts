import { Component, OnInit, ViewEncapsulation } from '@angular/core';

import { SidenavService } from '../../../core/services/sidenav.service';

@Component({
  selector: 'tb-topbar',
  template: "<md-toolbar color=\"primary\"> <md-icon id=\"toggle-sidebar\" (click)=\"toggleSidenav()\">menu</md-icon> <div class=\"topbar-title\"> <ng-content select=\".topbar-title-content\"></ng-content> </div> <div class=\"fill-remaining-space\"> <ng-content select=\".topbar-center\"></ng-content> </div> <div class=\"topbar-menu\"> <ng-content select=\".topbar-menu-content\"></ng-content> </div> </md-toolbar>",
  styles: [".fill-remaining-space { flex: 1 1 auto; text-align: center; } #toggle-sidebar { cursor: pointer; margin-right: 10px; } tb-topbar md-toolbar.md-primary { min-height: 53px; } tb-topbar md-toolbar md-toolbar-row { height: 53px !important; min-height: 53px !important; } tb-topbar .topbar-title { margin-left: 5px; } tb-topbar .topbar-title h1, tb-topbar .topbar-title h2 { margin: 0; padding: 0; } tb-topbar .topbar-title h1 { font-size: 22px; color: #fff; } tb-topbar .topbar-title h2 { font-size: 12px; font-weight: normal; } tb-topbar .mat-toolbar { min-height: 55px; } "],
  encapsulation: ViewEncapsulation.None
})
export class TopbarComponent implements OnInit {

  constructor(private sidenavService: SidenavService) { }

  ngOnInit() {
  }

  toggleSidenav() {
    this.sidenavService.toggleSidenav();
  }

}
