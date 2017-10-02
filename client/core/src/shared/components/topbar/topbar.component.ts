import { Component, OnInit, ViewEncapsulation } from '@angular/core';

import { SidenavService } from './../../../core/services/sidenav.service';

@Component({
  selector: 'tb-topbar',
  templateUrl: './topbar.component.html',
  styleUrls: ['./topbar.component.scss'],
  encapsulation: ViewEncapsulation.None
})

export class TopbarComponent implements OnInit {

  constructor(public sidenavService: SidenavService) { }

  ngOnInit() {
  }

  toggleSidenav() {
    this.sidenavService.toggleSidenav();
  }

}
