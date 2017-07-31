import { CommandEventArgs } from './../../../../models/eventargs.model';
import { Component, OnInit, OnDestroy } from '@angular/core';
import { Subscription } from 'rxjs';

import { EventDataService } from './../../../../../core/services/eventdata.service';
import { LoginSessionService } from './../../../../../core/services/login-session.service';
import { ContextMenuItem } from './../../../../models/context-menu-item.model';

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


        this.commandSubscription = this.eventDataService.command.subscribe((args: CommandEventArgs) => {
            switch (args.commandId) {
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
