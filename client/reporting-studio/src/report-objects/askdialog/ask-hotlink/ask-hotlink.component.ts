import { hotlink } from './../../../models/hotlink.model';
import { HttpService, LayoutService } from '@taskbuilder/core';
import { HotlinkComponent, EnumsService } from '@taskbuilder/core';
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
    protected layoutService: LayoutService,
    protected enumService: EnumsService
  ) {
    super(http, layoutService, enumService);
  }

  ngOnInit() {
    this.value = this.hotlink.value;
    this.selectionType = this.hotlink.selection_type;
  }
}