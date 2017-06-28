import { Component, OnDestroy } from '@angular/core';

import { MdDialog, MdDialogRef, MdDialogConfig } from '@angular/material';

import { ContextMenuItem } from '../../../../../shared';
import { UtilsService } from '../../../../services/utils.service';
import { EventDataService } from '../../../../services/eventdata.service';

import { ConnectionInfoDialogComponent, ProductInfoDialogComponent } from '../../../../../menu';
import { MenuService } from '../../../../../menu/services/menu.service';
import { HttpMenuService } from '../../../../../menu/services/http-menu.service';
import { LocalizationService } from '../../../../../menu/services/localization.service';

@Component({
  selector: 'tb-topbar-menu-app',
  templateUrl: './topbar-menu-app.component.html',
  styleUrls: ['./topbar-menu-app.component.scss']
})
export class TopbarMenuAppComponent implements OnDestroy {
  menuElements: ContextMenuItem[] = new Array<ContextMenuItem>();

  private show = false;

  viewProductInfo: string;
  productInfoDialogRef: MdDialogRef<ProductInfoDialogComponent>;
  connectionInfoDialogRef: MdDialogRef<ConnectionInfoDialogComponent>;
  data: Array<any>;
  localizationsLoadedSubscription: any;

  constructor(
    public dialog: MdDialog,
    private httpMenuService: HttpMenuService,
    private menuService: MenuService,
    private utilsService: UtilsService,
    private localizationService: LocalizationService,
    private eventDataService: EventDataService
  ) {

    this.localizationsLoadedSubscription = localizationService.localizationsLoaded.subscribe(() => {
      const item1 = new ContextMenuItem(this.localizationService.localizedElements.ViewProductInfo, 'idViewProductInfoButton', true, false);
      const item2 = new ContextMenuItem(this.localizationService.localizedElements.ConnectionInfo, 'idConnectionInfoButton', true, false);
      const item3 = new ContextMenuItem(this.localizationService.localizedElements.GotoProducerSite, 'idGotoProducerSiteButton', true, false);
      const item4 = new ContextMenuItem(this.localizationService.localizedElements.ClearCachedData, 'idClearCachedDataButton', true, false);
      const item5 = new ContextMenuItem(this.localizationService.localizedElements.ActivateViaSMS, 'idActivateViaSMSButton', true, false);
      // const item6 = new MenuItem(this.localizationService.localizedElements.ActivateViaInternet, 'idActivateViaInternetButton', true, false);
      this.menuElements.push(item1, item2, item3, item4, item5/*, item6*/);
    });



    this.eventDataService.command.subscribe((cmpId: string) => {
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
    this.httpMenuService.clearCachedData().subscribe(result => {
      location.reload();
    });
  }

  openProductInfoDialog() {
    this.productInfoDialogRef = this.dialog.open(ProductInfoDialogComponent, <MdDialogConfig>{});
  }

  openConnectionInfoDialog() {
    this.connectionInfoDialogRef = this.dialog.open(ConnectionInfoDialogComponent, <MdDialogConfig>{});
  }
}