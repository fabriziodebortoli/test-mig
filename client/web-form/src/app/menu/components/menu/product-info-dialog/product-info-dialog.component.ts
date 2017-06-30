import { UtilsService } from '@taskbuilder/core';
import { MaterialModule, MdDialog, MdDialogRef } from '@angular/material';
import { HttpMenuService } from '@taskbuilder/core';
import { LocalizationService } from '@taskbuilder/core';
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

