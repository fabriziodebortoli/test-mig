import { SidenavService } from './../../services/sidenav.service';
import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'tb-toolbar',
  templateUrl: './toolbar.component.html',
  styleUrls: ['./toolbar.component.css']
})
export class ToolbarComponent implements OnInit {

  constructor(private sidenavService: SidenavService) {

  }

  ngOnInit() {
  }

  toggleSidenav() {
    this.sidenavService.toggleSidenav();
  }

}
