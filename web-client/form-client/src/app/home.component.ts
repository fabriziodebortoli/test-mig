import { SidenavService } from './core/sidenav.service';
import { LoginSessionService, ComponentService } from 'tb-core';
import { Router } from '@angular/router';
import { Component, OnInit, ViewChild, OnDestroy } from '@angular/core';

@Component({
  selector: 'tb-home',
  templateUrl: './home.component.html',
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
    this.sidenavSubscription = sidenavService.sidenavOpened$.subscribe(
      sidebarOpened => {
        if (sidebarOpened) {
          this.sidenav.close();
        } else {
          this.sidenav.open();
        }

      });
  }

  ngOnInit() {
  }
  
  ngOnDestroy() {
    this.sidenavSubscription.unsubscribe();
  }
}
