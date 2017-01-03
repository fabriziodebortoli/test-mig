import { Component, OnInit, ViewChild, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';

import { SidenavService, LoginSessionService, ComponentService } from './../core';

@Component({
  selector: 'tb-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent implements OnInit, OnDestroy {

  @ViewChild('sidenav') sidenav;
  sidenavSubscription: any;

  constructor(
    private loginSession: LoginSessionService,
    private componentService: ComponentService,
    private sidenavService: SidenavService,
    private router: Router
  ) {
    this.sidenavSubscription = sidenavService.sidenavOpened$.subscribe(() => this.sidenav.toggle());
  }

  ngOnInit() {
  }

  ngOnDestroy() {
    this.sidenavSubscription.unsubscribe();
  }
}
