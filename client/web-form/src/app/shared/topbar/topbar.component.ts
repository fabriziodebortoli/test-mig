import { SidenavService } from '@taskbuilder/core';
import { Component, OnInit, ViewEncapsulation } from '@angular/core';

@Component({
  selector: 'tb-topbar',
  templateUrl: './topbar.component.html',
  styleUrls: ['./topbar.component.scss'],
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
