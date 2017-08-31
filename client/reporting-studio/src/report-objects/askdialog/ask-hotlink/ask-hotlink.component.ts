import { hotlink } from './../../../models/hotlink.model';
import { HttpService, LayoutService, TbComponentService, HotlinkComponent, EnumsService } from '@taskbuilder/core';
import { Observable } from 'rxjs/Rx';

import { Component, Input, DoCheck, KeyValueDiffers, Output, EventEmitter, ViewChild, ViewEncapsulation, OnInit } from '@angular/core';

@Component({
  selector: 'rs-ask-hotlink',
  templateUrl: './ask-hotlink.component.html',
  styleUrls: ['./ask-hotlink.component.scss'],
  encapsulation: ViewEncapsulation.None
})
export class AskHotlinkComponent extends HotlinkComponent implements OnInit {


  @Input() hotlink: hotlink;
  constructor(http: HttpService,
    layoutService: LayoutService,
    protected enumService: EnumsService,
    tbComponentService: TbComponentService
  ) {
    super(http, layoutService, enumService, tbComponentService);
  }

  ngOnInit() {
    this.value = this.hotlink.value;
    this.selectionType = this.hotlink.selection_type;
  }
}