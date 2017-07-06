import { HttpService } from '@taskbuilder/core';
import { HotlinkComponent } from '@taskbuilder/core';
import { Observable } from 'rxjs/Rx';

import { hotlink, CommandType } from './../../../reporting-studio.model';
import { Component, Input, DoCheck, KeyValueDiffers, Output, EventEmitter, ViewChild, ViewEncapsulation, OnInit } from '@angular/core';

@Component({
  selector: 'rs-ask-hotlink',
  templateUrl: './ask-hotlink.component.html',
  styleUrls: ['./ask-hotlink.component.scss'],
  encapsulation: ViewEncapsulation.None
})
export class AskHotlinkComponent extends HotlinkComponent implements OnInit {


  @Input() hotlink: hotlink;
  constructor(http: HttpService) {
    super(http);
  }

  ngOnInit() {
    this.value = this.hotlink.value;
    this.selectionType = this.hotlink.selection_type;
  }
}