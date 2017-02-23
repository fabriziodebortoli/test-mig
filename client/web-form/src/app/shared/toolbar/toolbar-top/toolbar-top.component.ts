import { EventDataService } from './../../../core/eventdata.service';
import { ViewModeType } from '../../';
import { Component } from '@angular/core';

@Component({
  selector: 'tb-toolbar-top',
  templateUrl: './toolbar-top.component.html',
  styleUrls: ['./toolbar-top.component.scss']
})
export class ToolbarTopComponent {

  private viewModeTypeModel = ViewModeType;

  constructor(private eventData: EventDataService) { }

}
