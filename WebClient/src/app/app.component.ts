import { LoginSessionService, WebSocketService, SidenavService, ComponentService } from 'tb-core';
import {Logger} from 'libclient';

import { TabComponent } from 'tb-shared';

import { Component, ViewChild, OnDestroy } from '@angular/core';

@Component({
  selector: 'tb-root',
  templateUrl: './app.component.html'
})
export class AppComponent implements OnDestroy {

  @ViewChild('sidenav') sidenav;

  sidenavSubscription: any;
  constructor(
    private loginSession: LoginSessionService,
    private socket: WebSocketService,
    private logger: Logger,
    private sidenavService: SidenavService,
    private componentService: ComponentService) {

    this.sidenavSubscription = sidenavService.sidenavOpened$.subscribe(
      sidebarOpened => {
        if (sidebarOpened) {
          this.sidenav.close();
        } else {
          this.sidenav.open();
        }

      });
  }

  ngOnDestroy() {
    this.sidenavSubscription.unsubscribe();
  }
  isConnected(): boolean {
    return this.loginSession.isConnected();
  }


  logout(): void {
    return this.loginSession.logout();
  }

  isWsConnected(): boolean {
    return this.socket.status === 'Open';
  }

  wsConnect(): void {
    this.socket.wsConnect();
  }

  wsClose(): void {
    this.socket.wsClose();
  }

  wsStatus(): string {
    return this.socket.status;
  }


  closeTab(tab: TabComponent) {
    //this.componentService.tryDestroyComponent(tab.componentInfo);
  }

  logoff(tab: TabComponent) {
    this.loginSession.logout();
  }
}
