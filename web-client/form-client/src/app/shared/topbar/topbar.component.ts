import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'tb-topbar',
  templateUrl: './topbar.component.html',
  styleUrls: ['./topbar.component.css']
})

export class TopbarComponent implements OnInit {

  constructor() {

  }

  ngOnInit() {
  }

  toggleSidenav() {
    // this.sidenavService.toggleSidenav();
  }

}
