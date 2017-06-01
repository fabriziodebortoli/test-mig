import { LoginSessionService } from './../../../../core/login-session.service';
import { Component, OnInit } from '@angular/core';
import { MenuItem } from './../../../context-menu/menu-item.model';
import { EventDataService } from './../../../../core/eventdata.service';

@Component({
  selector: 'tb-topbar-menu-user',
  templateUrl: './topbar-menu-user.component.html',
  styleUrls: ['./topbar-menu-user.component.scss']
})
export class TopbarMenuUserComponent {
 menuElements: MenuItem[] = new Array<MenuItem>();


  constructor(private loginSessionService: LoginSessionService,  private eventDataService: EventDataService) {
    const item1 = new MenuItem('Refresh', 'idRefreshButton', true, false);
    const item2 = new MenuItem('Settings', 'idSettingsButton', true, false);
    const item3 = new MenuItem('Help', 'idHelpButton', true, false);
    const item4 = new MenuItem('Sign Out', 'idSignOutButton', true, false);
    this.menuElements.push(item1, item2, item3, item4);


    this.eventDataService.command.subscribe((cmpId: string) => {
       switch (cmpId) {
      case 'idSignOutButton':
        return this.logout();
      default:
        break;
    }
  });
}
  logout() {
    this.loginSessionService.logout();
  }
}
