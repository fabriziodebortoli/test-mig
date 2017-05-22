import { UtilsService } from './../../../../core/utils.service';
import { MenuService } from './../../../../menu/services/menu.service';
import { HttpMenuService } from './../../../../menu/services/http-menu.service';
import { ConnectionInfoDialogComponent } from './../../../../menu/components/menu/connection-info-dialog/connection-info-dialog.component';
import { ProductInfoDialogComponent } from './../../../../menu/components/menu/product-info-dialog/product-info-dialog.component';
import { LocalizationService } from './../../../../menu/services/localization.service';
import { MdDialog, MdDialogRef, MdDialogConfig } from '@angular/material';
import { Component, OnInit, AfterViewInit, OnDestroy } from '@angular/core';
import { MenuItem } from './../../../context-menu/menu-item.model';
import { Collision } from '@progress/kendo-angular-popup/dist/es/models/collision.interface';
import { Align } from '@progress/kendo-angular-popup/dist/es/models/align.interface';

@Component({
  selector: 'tb-topbar-menu-app',
  templateUrl: './topbar-menu-app.component.html',
  styleUrls: ['./topbar-menu-app.component.css']
})
export class TopbarMenuAppComponent implements  OnDestroy {
anchorAlign: Align = { horizontal: 'right', vertical: 'bottom' };
  popupAlign: Align = { horizontal: 'right', vertical: 'top' };
  private collision: Collision = { horizontal: 'flip', vertical: 'fit' };
  contextMenu: MenuItem[] = new Array<MenuItem>();

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
    private localizationService: LocalizationService
  ) {

    this.localizationsLoadedSubscription = localizationService.localizationsLoaded.subscribe(() => {
const item1 = new MenuItem(this.localizationService.getLocalizedElement('ViewProductInfo'), 'idViewProductInfoButton', false, false);
  const item2 = new MenuItem(this.localizationService.getLocalizedElement('ConnectionInfo'), 'idConnectionInfoButton', false,  false);
  const item3 = new MenuItem(this.localizationService.getLocalizedElement('GotoProducerSite'), 'idGotoProducerSiteButton', false,  false);
  const item4 = new MenuItem(this.localizationService.getLocalizedElement('ClearCachedData'), 'idClearCachedDataButton', false,  false);
  const item5 = new MenuItem( this.localizationService.getLocalizedElement('ActivateViaSMS'), 'idActivateViaSMSButton', false,  false);
  const item6 = new MenuItem(this.localizationService.getLocalizedElement('ActivateViaInternet'),
  'idActivateViaInternetButton', false,  false);
 this.contextMenu.push(item1, item2, item3, item4, item5, item6);
   });
  }

   chooseAction(buttonID: string) {
      switch (buttonID) {
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
            case 'idActivateViaInternetButton':
          return this.activateViaInternet();
      
        default:
          break;
      }
   }

  ngOnDestroy() {
    this.localizationsLoadedSubscription.unsubscribe();
  }

  //---------------------------------------------------------------------------------------------
  activateViaSMS() {
    this.httpMenuService.activateViaSMS();
  }

  //---------------------------------------------------------------------------------------------
  goToSite() {
    this.httpMenuService.goToSite();
  }

  //---------------------------------------------------------------------------------------------
  activateViaInternet() {
    this.httpMenuService.activateViaInternet();
  }

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
