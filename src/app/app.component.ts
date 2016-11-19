import { TabComponent } from './core/components/tabber/tab.component';
import { ComponentService } from './core/services/component.service';
import { SidenavService } from './core/services/sidenav.service';
import { Component, ViewChild } from '@angular/core';

import { LoginSessionService, Logger, WebSocketService } from './core/services';

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

  createComponent(url: string) {
    this.componentService.createComponentFromUrl(url);
  }

  onCloseTab(tab: TabComponent) {
    this.componentService.removeComponent(tab.componentInfo);
  }
}
