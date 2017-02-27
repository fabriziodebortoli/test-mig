import { SidenavService } from '../../core/sidenav.service';
import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'tb-topbar',
  templateUrl: './topbar.component.html',
  styleUrls: ['./topbar.component.scss']
})

export class TopbarComponent implements OnInit {

  constructor(private sidenavService: SidenavService) { }

  ngOnInit() {
  }

  toggleSidenav() {
    this.sidenavService.toggleSidenav();
  }

}
