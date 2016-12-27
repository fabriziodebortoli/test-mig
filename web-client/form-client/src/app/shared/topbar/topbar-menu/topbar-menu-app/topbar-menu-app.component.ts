import { ConnectionInfoDialogComponent } from './../../../../menu/components/menu/connection-info-dialog/connection-info-dialog.component';
import { ProductInfoDialogComponent } from './../../../../menu/components/menu/product-info-dialog/product-info-dialog.component';
import { LocalizationService } from './../../../../menu/services/localization.service';
import { MdDialog, MdDialogRef, MdDialogConfig } from '@angular/material';
import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'tb-topbar-menu-app',
  templateUrl: './topbar-menu-app.component.html',
  styleUrls: ['./topbar-menu-app.component.css']
})
export class TopbarMenuAppComponent implements OnInit {

  constructor() { }

  ngOnInit() {
  }

  activateViaSMS() {
  }

  goToSite() {
  }

  activateViaInternet() {
  }

  clearCachedData() {
  }

  openProductInfoDialog() {
  }

  openConnectionInfoDialog() {
  }

}
