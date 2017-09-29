import { HotlinkComponent, HttpService, LayoutService, EnumsService, TbComponentService } from '@taskbuilder/core';
import { Component, ViewEncapsulation, OnInit, Input, ChangeDetectorRef } from '@angular/core';

import { hotlink } from './../../../models/hotlink.model';
import { Observable } from 'rxjs/Rx';



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
    tbComponentService: TbComponentService,
    cd: ChangeDetectorRef
  ) {
    super(http, layoutService, enumService, tbComponentService, cd);
  }

  ngOnInit() {
    this.value = this.hotlink.value;
    this.selectionType = this.hotlink.selection_type;
  }
}