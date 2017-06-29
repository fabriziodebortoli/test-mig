import { Subscription } from 'rxjs';
import { LoginSessionService } from '@taskbuilder/core';
import { Component, OnInit, OnDestroy } from '@angular/core';

import { EventDataService, ContextMenuItem } from '@taskbuilder/core';

@Component({
  selector: 'tb-topbar-menu-user',
  templateUrl: './topbar-menu-user.component.html',
  styleUrls: ['./topbar-menu-user.component.scss']
})
export class TopbarMenuUserComponent implements OnDestroy {
  menuElements: ContextMenuItem[] = new Array<ContextMenuItem>();

  commandSubscription: Subscription;
  constructor(private loginSessionService: LoginSessionService, private eventDataService: EventDataService) {
    const item1 = new ContextMenuItem('Refresh', 'idRefreshButton', true, false);
    const item2 = new ContextMenuItem('Settings', 'idSettingsButton', true, false);
    const item3 = new ContextMenuItem('Help', 'idHelpButton', true, false);
    const item4 = new ContextMenuItem('Sign Out', 'idSignOutButton', true, false);
    this.menuElements.push(item1, item2, item3, item4);


    this.commandSubscription = this.eventDataService.command.subscribe((cmpId: string) => {
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

  ngOnDestroy() {

    this.commandSubscription.unsubscribe();
  }
}
