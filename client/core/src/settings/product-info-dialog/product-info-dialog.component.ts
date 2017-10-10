import { UtilsService } from './../../core/services/utils.service';
import { InfoService } from './../../core/services/info.service';
import { LocalizationService } from './../../menu/services/localization.service';
import { Component, OnInit } from '@angular/core';


@Component({
  selector: 'tb-product-info-dialog',
  templateUrl: './product-info-dialog.component.html',
  styleUrls: ['./product-info-dialog.component.scss']
})

export class ProductInfoDialogComponent implements OnInit {

  public productInfos: any;
  constructor(
      public infoService: InfoService,
    public utilsService: UtilsService,
    public localizationService: LocalizationService
  ) {

  }

  ngOnInit() {

    this.localizationService.localizationsLoaded.subscribe((loaded) => {
      console.log("loaded", loaded );
      if (!loaded)
        return;
    });


    let sub = this.infoService.getProductInfo().subscribe(result => {
      this.productInfos = result;
      if (sub)
        sub.unsubscribe()
    });
  }
}

