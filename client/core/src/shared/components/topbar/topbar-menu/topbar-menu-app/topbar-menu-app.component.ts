import { CommandEventArgs } from './../../../../models/eventargs.model';
import { Component, OnInit, AfterViewInit, OnDestroy } from '@angular/core';

import { MdDialog, MdDialogRef, MdDialogConfig } from '@angular/material';

import { EventDataService } from './../../../../../core/services/eventdata.service';
import { UtilsService } from './../../../../../core/services/utils.service';
import { ContextMenuItem } from './../../../../models/context-menu-item.model';

import { LocalizationService } from './../../../../../menu/services/localization.service';
import { MenuService } from './../../../../../menu/services/menu.service';
import { HttpMenuService } from './../../../../../menu/services/http-menu.service';
import { ConnectionInfoDialogComponent } from './../../../../../menu/components/menu/connection-info-dialog/connection-info-dialog.component';
//import { ProductInfoDialogComponent } from './../../../../../menu/components/menu/product-info-dialog/product-info-dialog.component';

@Component({
    selector: 'tb-topbar-menu-app',
    templateUrl: './topbar-menu-app.component.html',
    styleUrls: ['./topbar-menu-app.component.scss']
})
export class TopbarMenuAppComponent implements OnDestroy {

    public menuElements: ContextMenuItem[] = new Array<ContextMenuItem>();
    public show = false;
    public viewProductInfo: string;
    public data: Array<any>;
    public localizationsLoadedSubscription: any;

    constructor(
        public dialog: MdDialog,
        public httpMenuService: HttpMenuService,
        public menuService: MenuService,
        public utilsService: UtilsService,
        public localizationService: LocalizationService,
        public eventDataService: EventDataService
    ) {

        this.localizationsLoadedSubscription = localizationService.localizationsLoaded.subscribe((loaded) => {
            if (!loaded)
                return;
            //const item1 = new ContextMenuItem(this.localizationService.localizedElements.ViewProductInfo, 'idViewProductInfoButton', true, false);
            const item2 = new ContextMenuItem(this.localizationService.localizedElements.ConnectionInfo, 'idConnectionInfoButton', true, false);
            const item3 = new ContextMenuItem(this.localizationService.localizedElements.GotoProducerSite, 'idGotoProducerSiteButton', true, false);
            const item4 = new ContextMenuItem(this.localizationService.localizedElements.ClearCachedData, 'idClearCachedDataButton', true, false);
            const item5 = new ContextMenuItem(this.localizationService.localizedElements.ActivateViaSMS, 'idActivateViaSMSButton', true, false);
            // const item6 = new MenuItem(this.localizationService.localizedElements.ActivateViaInternet, 'idActivateViaInternetButton', true, false);
            this.menuElements.push(/*item1,*/ item2, item3, item4, item5/*, item6*/);
        });

        this.eventDataService.command.subscribe((args: CommandEventArgs) => {
            switch (args.commandId) {
                // case 'idViewProductInfoButton':
                //     return this.openProductInfoDialog();
                case 'idConnectionInfoButton':
                    return this.openConnectionInfoDialog();
                case 'idGotoProducerSiteButton':
                    return this.goToSite();
                case 'idClearCachedDataButton':
                    return this.clearCachedData();
                case 'idActivateViaSMSButton':
                    return this.activateViaSMS();
                // case 'idActivateViaInternetButton':
                //   return this.activateViaInternet();
                default:
                    break;
            }
        });
    }

    ngOnDestroy() {
        this.localizationsLoadedSubscription.unsubscribe();
    }

    //---------------------------------------------------------------------------------------------
    activateViaSMS() {
        this.httpMenuService.activateViaSMS().subscribe((result) => {
            window.open(result.url, "_blank");
        });

    }

    //---------------------------------------------------------------------------------------------
    goToSite() {
        this.httpMenuService.goToSite().subscribe((result) => {
            window.open(result.url, "_blank");
        });
    }

    // //---------------------------------------------------------------------------------------------
    // activateViaInternet() {
    //   this.httpMenuService.activateViaInternet();
    // }

    clearCachedData() {
        this.menuService.invalidateCache();
    }

    // openProductInfoDialog() {
    //     this.dialog.open(ProductInfoDialogComponent, <MdDialogConfig>{});
    // }

    openConnectionInfoDialog() {
        this.dialog.open(ConnectionInfoDialogComponent, <MdDialogConfig>{});
    }
}
