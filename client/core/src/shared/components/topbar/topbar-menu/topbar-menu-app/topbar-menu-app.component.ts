import { CommandEventArgs } from './../../../../models/eventargs.model';
import { Component, OnInit, AfterViewInit, OnDestroy, ChangeDetectorRef } from '@angular/core';

import { EventDataService } from './../../../../../core/services/eventdata.service';
import { UtilsService } from './../../../../../core/services/utils.service';
import { ContextMenuItem } from './../../../../models/context-menu-item.model';

import { MenuService } from './../../../../../menu/services/menu.service';
import { HttpMenuService } from './../../../../../menu/services/http-menu.service';
import { TbComponentService } from './../../../../../core/services/tbcomponent.service';
import { TbComponent } from './../../../../../shared/components/tb.component';

@Component({
    selector: 'tb-topbar-menu-app',
    templateUrl: './topbar-menu-app.component.html',
    styleUrls: ['./topbar-menu-app.component.scss']
})
export class TopbarMenuAppComponent extends TbComponent implements OnDestroy {

    public menuElements: ContextMenuItem[] = new Array<ContextMenuItem>();
    public show = false;
    public viewProductInfo: string;
    public data: Array<any>;
    private eventDataServiceSubscription;
    constructor(
        public httpMenuService: HttpMenuService,
        public menuService: MenuService,
        public utilsService: UtilsService,
        public eventDataService: EventDataService,
        tbComponentService: TbComponentService,
        changeDetectorRef: ChangeDetectorRef
    ) {
        super(tbComponentService, changeDetectorRef);
        this.enableLocalization();


        this.eventDataServiceSubscription = this.eventDataService.command.subscribe((args: CommandEventArgs) => {
            switch (args.commandId) {
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
    onTranslationsReady() {
        super.onTranslationsReady();
        this.menuElements.splice(0, this.menuElements.length);
        const item1 = new ContextMenuItem(this._TB('Producer Site'), 'idGotoProducerSiteButton', true, false);
        const item2 = new ContextMenuItem(this._TB('Clear cached data'), 'idClearCachedDataButton', true, false);
        const item3 = new ContextMenuItem(this._TB('Activate via SMS'), 'idActivateViaSMSButton', true, false);
        this.menuElements.push(item1, item2, item3);
    }
    ngOnDestroy() {
        this.eventDataServiceSubscription.unsubscribe();
    }

    //---------------------------------------------------------------------------------------------
    activateViaSMS() {
        let subs = this.httpMenuService.activateViaSMS().subscribe((result) => {
            subs.unsubscribe();
            window.open(result.url, "_blank");
        });

    }

    //---------------------------------------------------------------------------------------------
    goToSite() {
        let subs = this.httpMenuService.goToSite().subscribe((result) => {
            subs.unsubscribe();
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
}
