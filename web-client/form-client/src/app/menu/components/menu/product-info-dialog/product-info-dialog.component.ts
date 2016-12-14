import { MaterialModule, MdDialog, MdDialogRef } from '@angular/material';
import { HttpMenuService } from './../../../services/http-menu.service';
import { MenuService } from './../../../services/menu.service';
import { UtilsService } from 'tb-core';
import { LocalizationService } from './../../../services/localization.service';
import { Component, OnInit } from '@angular/core';

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
    private menuService: MenuService,
    private utilsService: UtilsService,
    private localizationService: LocalizationService
  ) {

  }

  ngOnInit() {
    this.httpMenuService.getProductInfo().subscribe(result=> { this.productInfos = result.ProductInfos; });
  }
}

