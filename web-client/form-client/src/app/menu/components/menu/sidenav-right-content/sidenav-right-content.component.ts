import { ConnectionInfoDialogComponent } from './../connection-info-dialog/connection-info-dialog.component';
import { ProductInfoDialogComponent } from './../product-info-dialog/product-info-dialog.component';
import { MaterialModule, MdDialog, MdDialogRef, MdDialogConfig } from '@angular/material';
import { HttpMenuService } from './../../../services/http-menu.service';
import { MenuService } from './../../../services/menu.service';
import { UtilsService } from 'tb-core';
import { LocalizationService } from './../../../services/localization.service';
import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'tb-sidenav-right-content',
  templateUrl: './sidenav-right-content.component.html',
  styleUrls: ['./sidenav-right-content.component.css']
})

export class RightSidenavComponent implements OnInit {

  productInfoDialogRef: MdDialogRef<ProductInfoDialogComponent>;
  connectionInfoDialogRef: MdDialogRef<ConnectionInfoDialogComponent>;

  constructor(
    public dialog: MdDialog,
    private httpMenuService: HttpMenuService,
    private menuService: MenuService,
    private utilsService: UtilsService,
    private localizationService: LocalizationService
  ) {
  }

  ngOnInit() {
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
    this.productInfoDialogRef = this.dialog.open(ProductInfoDialogComponent, <MdDialogConfig>{ });
  }
   
   openConnectionInfoDialog() {
    this.connectionInfoDialogRef = this.dialog.open(ConnectionInfoDialogComponent, <MdDialogConfig>{ });
  }
};



