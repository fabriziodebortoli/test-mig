import { Component, OnInit, ChangeDetectorRef } from '@angular/core';

import { TbComponent, InfoService, UtilsService, TbComponentService } from '@taskbuilder/core';

@Component({
  selector: 'tb-product-info',
  templateUrl: './product-info.component.html',
  styleUrls: ['./product-info.component.scss']
})
export class ProductInfoComponent extends TbComponent implements OnInit {

  public productInfos: any;

  constructor(
    public infoService: InfoService,
    public utilsService: UtilsService,
    tbComponentService: TbComponentService,
    changeDetectorRef: ChangeDetectorRef
  ) {
    super(tbComponentService, changeDetectorRef);
    this.enableLocalization();
  }

  ngOnInit() {
    super.ngOnInit();
    let sub = this.infoService.getProductInfo(true).subscribe(result => {
      this.productInfos = result;
      if (sub)
        sub.unsubscribe()
    });
  }
}

