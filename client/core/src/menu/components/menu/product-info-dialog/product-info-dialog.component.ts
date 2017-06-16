import { Component, OnInit } from '@angular/core';
import { MdDialogRef } from '@angular/material';

import { UtilsService } from './../../../../core/services/utils.service';
import { HttpMenuService } from './../../../services/http-menu.service';
import { LocalizationService } from './../../../services/localization.service';

@Component({
  selector: 'tb-product-info-dialog',
  templateUrl: './product-info-dialog.component.html',
  styleUrls: ['./product-info-dialog.component.css']
})

export class ProductInfoDialogComponent implements OnInit {

private productInfos: any;
  constructor(
    public dialogRef: MdDialogRef<ProductInfoDialogComponent>,
    private httpMenuService: HttpMenuService,
    private utilsService: UtilsService,
    private localizationService: LocalizationService
  ) {

  }

  ngOnInit() {
   let sub = this.httpMenuService.getProductInfo().subscribe(result=> { 
     this.productInfos = result.ProductInfos; sub.unsubscribe()
     });
  }
}

