import { InfoService } from './../../../../core/services/info.service';
import { Component, OnInit } from '@angular/core';

import { MaterialModule, MdDialog, MdDialogRef } from '@angular/material';

import { LocalizationService } from './../../../services/localization.service';
import { UtilsService } from './../../../../core/services/utils.service';
import { HttpMenuService } from './../../../services/http-menu.service';

@Component({
  selector: 'tb-product-info-dialog',
  templateUrl: './product-info-dialog.component.html',
  styleUrls: ['./product-info-dialog.component.css']
})

export class ProductInfoDialogComponent implements OnInit {

  public productInfos: any;
  constructor(
    public dialogRef: MdDialogRef<ProductInfoDialogComponent>,
    public infoService: InfoService,
    public utilsService: UtilsService,
    public localizationService: LocalizationService
  ) {

  }

  ngOnInit() {
    let sub = this.infoService.getProductInfo().subscribe(result => {
      this.productInfos = result;
      if (sub)
        sub.unsubscribe()
    });
  }
}

