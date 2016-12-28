import { environment } from '../environments/environment';
import { SidenavService } from './core/sidenav.service';
import { LoginSessionService, ComponentService } from 'tb-core';
import { Router } from '@angular/router';
import { Component, OnInit, ViewChild, OnDestroy } from '@angular/core';

@Component({
  selector: 'tb-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent implements OnInit, OnDestroy {

  @ViewChild('sidenav') sidenav;
  sidenavSubscription: any;

  private appName = environment.appName;
  private companyName = environment.companyName;

  constructor(
    private loginSession: LoginSessionService,
    private componentService: ComponentService,
    private sidenavService: SidenavService,
    private router: Router
  ) {
    this.sidenavSubscription = sidenavService.sidenavOpened$.subscribe(
      sidebarOpened => {
        console.log("sidebarOpened: ", sidebarOpened);
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

  closeSidenav() {
    this.sidenav.close();
  }
}
