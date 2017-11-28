import { DiagnosticService } from './../../../../../core/services/diagnostic.service';
import { DiagnosticItemComponent } from './../../../../containers/diagnostic-dialog/diagnostic-dialog.component';
import { OldLocalizationService } from './../../../../../core/services/oldlocalization.service';
import { SettingsService } from './../../../../../core/services/settings.service';
import { HttpService } from './../../../../../core/services/http.service';
import { InfoService } from './../../../../../core/services/info.service';
import { HttpMenuService } from './../../../../../menu/services/http-menu.service';
import { ComponentService } from './../../../../../core/services/component.service';
import { Component, OnInit, OnDestroy } from '@angular/core';
import { Subscription } from '../../../../../rxjs.imports';

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
    localizationsLoadedSubscription: Subscription;
    constructor(
        public componentService: ComponentService,
        public authService: AuthService,
        public eventDataService: EventDataService,
        public httpMenuService: HttpMenuService,
        public infoService: InfoService,
        public httpService: HttpService,
        public settingsService: SettingsService,
        public localizationService: OldLocalizationService,
        public diagnosticService: DiagnosticService
    ) {
        this.localizationsLoadedSubscription = localizationService.localizationsLoaded.subscribe((loaded) => {
            if (!loaded || !this.localizationService.localizedElements)
                return;

            const item2 = new ContextMenuItem(this.localizationService.localizedElements.Settings, 'idSettingsButton', true, false);
            const item3 = new ContextMenuItem(this.localizationService.localizedElements.Help, 'idHelpButton', true, false);
            const item4 = new ContextMenuItem(this.localizationService.localizedElements.Logout, 'idSignOutButton', true, false);
            this.menuElements.push(/*item1, */item2, item3, item4);
        });

        this.commandSubscription = this.eventDataService.command.subscribe((args: CommandEventArgs) => {

            switch (args.commandId) {
                case 'idSignOutButton':
                    return this.logout();
                case 'idSettingsButton':
                    return this.openSettingsPage();
                case 'idHelpButton':
                    return this.openHelp();
                default:
                    break;
            }
        });
    }

    logout() {
        let subs = this.httpService.canLogoff({ authtoken: sessionStorage.getItem('authtoken') }).subscribe((res) => {
            if (!res.error) {
                this.authService.logout();
            }
            else {
                this.diagnosticService.showDiagnostic(res.messages);
            }
            subs.unsubscribe();
        });
    }

    ngOnDestroy() {
        this.commandSubscription.unsubscribe();
        this.localizationsLoadedSubscription.unsubscribe();
    }

    openHelp() {
        let ns = "RefGuide.Menu"
        let subs = this.httpMenuService.callonlineHelpUrl(ns, "").subscribe((res) => {
            subs.unsubscribe();
            if (res.url)
                window.open(res.url, '_blank');
        });  //TODOLUCA culture da impostare
    }

    openSettingsPage() {
        this.settingsService.settingsPageOpenedEvent.emit(true);
        //this.componentService.createComponentFromUrl('settings/settings', true);
    }
}
