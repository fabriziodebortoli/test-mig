import { HotlinkComponent } from './../../../../shared/controls/hotlink/hotlink.component';
import { HttpService } from './../../../../core/http.service';
import { Observable } from 'rxjs/Rx';
import { ReportingStudioService } from './../../../reporting-studio.service';
import { hotlink, CommandType } from './../../../reporting-studio.model';
import { Component, Input, DoCheck, KeyValueDiffers, Output, EventEmitter, ViewChild, ViewEncapsulation } from '@angular/core';

@Component({
  selector: 'rs-ask-hotlink',
  templateUrl: './ask-hotlink.component.html',
  styleUrls: ['./ask-hotlink.component.scss'],
  encapsulation: ViewEncapsulation.None
})
export class AskHotlinkComponent extends HotlinkComponent {


  @Input() hotlink: hotlink;
  constructor(http: HttpService) {
    super(http);
    this.value = this.hotlink.value;
    this.selectionType = this.hotlink.selection_type;
  }
}