import { Component, OnInit, ViewChild, OnDestroy } from '@angular/core';
import { environment } from './../../environments/environment';

import { SidenavService, LoginSessionService, ComponentService } from 'tb-core';

@Component({
  selector: 'tb-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent implements OnInit, OnDestroy {

  @ViewChild('sidenav') sidenav;
  sidenavSubscription: any;

  private appName = environment.appName;
  private companyName = environment.companyName;

  constructor(
    private sidenavService: SidenavService,
    private loginSession: LoginSessionService,
    private componentService: ComponentService,

  ) {
    this.sidenavSubscription = sidenavService.sidenavOpened$.subscribe(() => this.sidenav.toggle());

  }

  ngOnInit() {

  }

  ngOnDestroy() {
    this.sidenavSubscription.unsubscribe();
  }

  toggleSidenav() {
    this.sidenavService.toggleSidenav();
  }
}
