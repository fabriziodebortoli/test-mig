import { SidenavService } from '../../core/sidenav.service';
import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'tb-topbar',
  templateUrl: './topbar.component.html',
  styleUrls: ['./topbar.component.css']
})

export class TopbarComponent implements OnInit {

  title: string = 'Mago Web';
  subtitle: string = 'Microarea Spa';

  constructor(private sidenavService: SidenavService) {

  }

  ngOnInit() {
  }

  toggleSidenav() {
    this.sidenavService.toggleSidenav();
  }

}
