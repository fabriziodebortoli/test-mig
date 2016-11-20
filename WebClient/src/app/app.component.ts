import { LoginSessionService, WebSocketService, Logger, SidenavService, ComponentService } from './core';
import {TabComponent} from './shared';

import { Component, ViewChild } from '@angular/core';

@Component({
  selector: 'tb-root',
  templateUrl: './app.component.html'
})
export class AppComponent {

  @ViewChild('sidenav') sidenav;

  constructor(
    private loginSession: LoginSessionService,
    private socket: WebSocketService,
    private logger: Logger,
    private sidenavService: SidenavService,
    private componentService: ComponentService) {

    sidenavService.sidenavOpened$.subscribe(
      sidebarOpened => {
        if (sidebarOpened) {
          this.sidenav.close();
        } else {
          this.sidenav.open();
        }

      });
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
    this.componentService.tryDestroyComponent(tab.componentInfo);
  }

  logoff(tab: TabComponent) {
    this.loginSession.logout();
  }
}
