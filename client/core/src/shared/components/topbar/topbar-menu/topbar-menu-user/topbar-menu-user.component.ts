import { ComponentService } from './../../../../../core/services/component.service';
import { Component, OnInit, OnDestroy } from '@angular/core';
import { Subscription } from 'rxjs';

import { CommandEventArgs } from './../../../../models/eventargs.model';
import { ContextMenuItem } from './../../../../models/context-menu-item.model';

import { AuthService } from './../../../../../core/services/auth.service';
import { EventDataService } from './../../../../../core/services/eventdata.service';

@Component({
    selector: 'tb-topbar-menu-user',
    templateUrl: './topbar-menu-user.component.html',
    styleUrls: ['./topbar-menu-user.component.scss']
})
export class TopbarMenuUserComponent implements OnDestroy {
    menuElements: ContextMenuItem[] = new Array<ContextMenuItem>();

    commandSubscription: Subscription;
    constructor(private componentService: ComponentService, private authService: AuthService, private eventDataService: EventDataService) {
        const item1 = new ContextMenuItem('Refresh', 'idRefreshButton', true, false);
        const item2 = new ContextMenuItem('Settings', 'idSettingsButton', true, false);
        const item3 = new ContextMenuItem('Help', 'idHelpButton', true, false);
        const item4 = new ContextMenuItem('Sign Out', 'idSignOutButton', true, false);
        this.menuElements.push(item1, item2, item3, item4);


        this.commandSubscription = this.eventDataService.command.subscribe((args: CommandEventArgs) => {
            switch (args.commandId) {
                case 'idSignOutButton':
                    return this.logout();
                case 'idSettingsButton':
                    return this.openSettingsPage();
                default:
                    break;
            }
        });
    }
    logout() {
        this.authService.logout();
    }

    openSettings()
    {
        this.componentService.createComponentFromUrl('settings', true);
    }
    ngOnDestroy() {

        this.commandSubscription.unsubscribe();
    }

    openSettingsPage(){
        this.componentService.createComponentFromUrl('settings/settings', true);
    }
}
