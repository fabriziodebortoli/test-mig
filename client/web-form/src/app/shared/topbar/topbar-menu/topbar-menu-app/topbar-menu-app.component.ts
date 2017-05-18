import { UtilsService } from './../../../../core/utils.service';
import { MenuService } from './../../../../menu/services/menu.service';
import { HttpMenuService } from './../../../../menu/services/http-menu.service';
import { ConnectionInfoDialogComponent } from './../../../../menu/components/menu/connection-info-dialog/connection-info-dialog.component';
import { ProductInfoDialogComponent } from './../../../../menu/components/menu/product-info-dialog/product-info-dialog.component';
import { LocalizationService } from './../../../../menu/services/localization.service';
import { MdDialog, MdDialogRef, MdDialogConfig } from '@angular/material';
import { Component, OnInit, AfterViewInit, OnDestroy } from '@angular/core';

@Component({
  selector: 'tb-topbar-menu-app',
  templateUrl: './topbar-menu-app.component.html',
  styleUrls: ['./topbar-menu-app.component.css']
})
export class TopbarMenuAppComponent implements  OnDestroy {

  private title: string = "App menu";
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

      this.data = [{
        actionName: this.localizationService.getLocalizedElement('ViewProductInfo'),
        click: (dataItem) => {
          this.openProductInfoDialog();
        }
      }, {
        actionName: this.localizationService.getLocalizedElement('ConnectionInfo'),
        click: (dataItem) => {
          this.openConnectionInfoDialog();
        }
      }, {
        actionName: this.localizationService.getLocalizedElement('ClearCachedData'),
        click: (dataItem) => {
          this.clearCachedData();
        }
      }, {
        actionName: this.localizationService.getLocalizedElement('ActivateViaSMS'),
        click: (dataItem) => {
          this.activateViaSMS();
        }
      }, {
        actionName: this.localizationService.getLocalizedElement('ActivateViaInternet'),
        click: (dataItem) => {
          this.activateViaInternet();
        }
      }];

    });
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
