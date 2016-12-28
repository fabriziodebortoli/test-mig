import { environment } from '../../../environments/environment';
import { SidenavService } from '../../core/sidenav.service';
import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'tb-topbar',
  templateUrl: './topbar.component.html',
  styleUrls: ['./topbar.component.css']
})

export class TopbarComponent implements OnInit {

  private appName = environment.appName;
  private companyName = environment.companyName;

  constructor(private sidenavService: SidenavService) {

  }

  ngOnInit() {
  }

  toggleSidenav() {
    this.sidenavService.toggleSidenav();
  }

}
