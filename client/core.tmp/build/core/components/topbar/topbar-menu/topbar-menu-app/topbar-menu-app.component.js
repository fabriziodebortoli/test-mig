import { Component } from '@angular/core';
import { MdDialog } from '@angular/material';
import { ContextMenuItem } from '../../../../../shared/index';
import { UtilsService } from '../../../../services/utils.service';
import { EventDataService } from '../../../../services/eventdata.service';
import { ConnectionInfoDialogComponent, ProductInfoDialogComponent } from '../../../../../menu/index';
import { MenuService } from '../../../../../menu/services/menu.service';
import { HttpMenuService } from '../../../../../menu/services/http-menu.service';
import { LocalizationService } from '../../../../../menu/services/localization.service';
export class TopbarMenuAppComponent {
    /**
     * @param {?} dialog
     * @param {?} httpMenuService
     * @param {?} menuService
     * @param {?} utilsService
     * @param {?} localizationService
     * @param {?} eventDataService
     */
    constructor(dialog, httpMenuService, menuService, utilsService, localizationService, eventDataService) {
        this.dialog = dialog;
        this.httpMenuService = httpMenuService;
        this.menuService = menuService;
        this.utilsService = utilsService;
        this.localizationService = localizationService;
        this.eventDataService = eventDataService;
        this.menuElements = new Array();
        this.show = false;
        this.localizationsLoadedSubscription = localizationService.localizationsLoaded.subscribe(() => {
            const item1 = new ContextMenuItem(this.localizationService.localizedElements.ViewProductInfo, 'idViewProductInfoButton', true, false);
            const item2 = new ContextMenuItem(this.localizationService.localizedElements.ConnectionInfo, 'idConnectionInfoButton', true, false);
            const item3 = new ContextMenuItem(this.localizationService.localizedElements.GotoProducerSite, 'idGotoProducerSiteButton', true, false);
            const item4 = new ContextMenuItem(this.localizationService.localizedElements.ClearCachedData, 'idClearCachedDataButton', true, false);
            const item5 = new ContextMenuItem(this.localizationService.localizedElements.ActivateViaSMS, 'idActivateViaSMSButton', true, false);
            // const item6 = new MenuItem(this.localizationService.localizedElements.ActivateViaInternet, 'idActivateViaInternetButton', true, false);
            this.menuElements.push(item1, item2, item3, item4, item5 /*, item6*/);
        });
        this.eventDataService.command.subscribe((cmpId) => {
            switch (cmpId) {
                case 'idViewProductInfoButton':
                    return this.openProductInfoDialog();
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
    /**
     * @return {?}
     */
    ngOnDestroy() {
        this.localizationsLoadedSubscription.unsubscribe();
    }
    /**
     * @return {?}
     */
    activateViaSMS() {
        this.httpMenuService.activateViaSMS().subscribe((result) => {
            window.open(result.url, "_blank");
        });
    }
    /**
     * @return {?}
     */
    goToSite() {
        this.httpMenuService.goToSite().subscribe((result) => {
            window.open(result.url, "_blank");
        });
    }
    /**
     * @return {?}
     */
    clearCachedData() {
        this.httpMenuService.clearCachedData().subscribe(result => {
            location.reload();
        });
    }
    /**
     * @return {?}
     */
    openProductInfoDialog() {
        this.productInfoDialogRef = this.dialog.open(ProductInfoDialogComponent, /** @type {?} */ ({}));
    }
    /**
     * @return {?}
     */
    openConnectionInfoDialog() {
        this.connectionInfoDialogRef = this.dialog.open(ConnectionInfoDialogComponent, /** @type {?} */ ({}));
    }
}
TopbarMenuAppComponent.decorators = [
    { type: Component, args: [{
                selector: 'tb-topbar-menu-app',
                template: "<tb-topbar-menu-elements [menuElements]=\"menuElements\" popupClass='popup'></tb-topbar-menu-elements>",
                styles: [""]
            },] },
];
/**
 * @nocollapse
 */
TopbarMenuAppComponent.ctorParameters = () => [
    { type: MdDialog, },
    { type: HttpMenuService, },
    { type: MenuService, },
    { type: UtilsService, },
    { type: LocalizationService, },
    { type: EventDataService, },
];
function TopbarMenuAppComponent_tsickle_Closure_declarations() {
    /** @type {?} */
    TopbarMenuAppComponent.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    TopbarMenuAppComponent.ctorParameters;
    /** @type {?} */
    TopbarMenuAppComponent.prototype.menuElements;
    /** @type {?} */
    TopbarMenuAppComponent.prototype.show;
    /** @type {?} */
    TopbarMenuAppComponent.prototype.viewProductInfo;
    /** @type {?} */
    TopbarMenuAppComponent.prototype.productInfoDialogRef;
    /** @type {?} */
    TopbarMenuAppComponent.prototype.connectionInfoDialogRef;
    /** @type {?} */
    TopbarMenuAppComponent.prototype.data;
    /** @type {?} */
    TopbarMenuAppComponent.prototype.localizationsLoadedSubscription;
    /** @type {?} */
    TopbarMenuAppComponent.prototype.dialog;
    /** @type {?} */
    TopbarMenuAppComponent.prototype.httpMenuService;
    /** @type {?} */
    TopbarMenuAppComponent.prototype.menuService;
    /** @type {?} */
    TopbarMenuAppComponent.prototype.utilsService;
    /** @type {?} */
    TopbarMenuAppComponent.prototype.localizationService;
    /** @type {?} */
    TopbarMenuAppComponent.prototype.eventDataService;
}
