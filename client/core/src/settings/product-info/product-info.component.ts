import { LocalizationService } from './../../core/services/localization.service';
import { UtilsService } from './../../core/services/utils.service';
import { InfoService } from './../../core/services/info.service';
import { Component, OnInit } from '@angular/core';


@Component({
  selector: 'tb-product-info',
  templateUrl: './product-info.component.html',
  styleUrls: ['./product-info.component.scss']
})

export class ProductInfoComponent implements OnInit {

  public productInfos: any;
  constructor(
    public infoService: InfoService,
    public utilsService: UtilsService,
    public localizationService: LocalizationService
  ) {

  }

  ngOnInit() {
    let sub = this.infoService.getProductInfo(true).subscribe(result => {
      this.productInfos = result;
      if (sub)
        sub.unsubscribe()
    });
  }
}

