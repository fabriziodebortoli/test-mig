import { Component, OnInit } from '@angular/core';
import { MdDialogRef } from '@angular/material';

import { UtilsService } from './../../../../core/services/utils.service';
import { HttpMenuService } from './../../../services/http-menu.service';
import { LocalizationService } from './../../../services/localization.service';

@Component({
  selector: 'tb-product-info-dialog',
  template: " <table> <tr> <td class=\"loginStyleOff pointer backButtonPadding\" ></td> <td> </td> </tr> <tr> <td style=\"text-align:right\"> {{localizationService.localizedElements?.InstallationVersion}}: </td> <td><b>{{productInfos?.installationVersion}} {{productInfos?.debugState}}</b></td> </tr> <tr> <td style=\"text-align:right\"> {{localizationService.localizedElements?.Provider}}: </td> <td><b>{{productInfos?.providerDescription}}</b></td> </tr> <tr> <td style=\"text-align:right\"> {{localizationService.localizedElements?.Edition}}: </td> <td><b>{{productInfos?.edition}}</b></td> </tr> <tr> <td style=\"text-align:right\"> {{localizationService.localizedElements?.InstallationName}}: </td> <td><b>{{productInfos?.installationName}}</b></td> </tr> <tr> <td style=\"text-align:right\"> {{localizationService.localizedElements?.ActivationState}}: </td> <td><b>{{productInfos?.activationState}}</b></td> </tr> <tr *ngFor=\"let current of utilsService.toArray(productInfos?.Applications)\"> <td style=\"text-align:right\"> {{current?.application}}: </td> <td> <b>{{current?.licensed}}</b> </td> </tr> </table> <button type=\"button\" (click)=\"dialogRef.close('yes')\">{{localizationService.localizedElements?.CloseLabel}}</button> ",
  styles: [""]
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

