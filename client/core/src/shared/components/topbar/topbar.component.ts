import { Component, OnInit, ViewEncapsulation } from '@angular/core';

import { SidenavService } from './../../../core/services/sidenav.service';

@Component({
  selector: 'tb-topbar',
  templateUrl: './topbar.component.html',
  styleUrls: ['./topbar.component.scss']
})

export class TopbarComponent implements OnInit {

  constructor(public sidenavService: SidenavService) { }

  ngOnInit() {
  }

  toggleSidenavLeft() {
    this.sidenavService.toggleSidenavLeft();
  }

  toggleSidenavRight() {
    this.sidenavService.toggleSidenavRight();
  }

}
