import { Component, OnInit, OnDestroy } from '@angular/core';

import { Subscription } from 'rxjs';

import { ContextMenuItem } from '../../../../../shared';
import { LoginSessionService } from '../../../../services/login-session.service';
import { EventDataService } from '../../../../services/eventdata.service';

@Component({
  selector: 'tb-topbar-menu-user',
  template: "<tb-topbar-menu-elements [menuElements]=\"menuElements\" [fontIcon]=\"'person'\" popupClass='popup'></tb-topbar-menu-elements>",
  styles: [""]
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
