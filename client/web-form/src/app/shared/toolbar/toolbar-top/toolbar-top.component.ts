import { Component, Input } from '@angular/core';

import { ViewModeType } from 'tb-shared';
import { EventDataService } from 'tb-core';

@Component({
  selector: 'tb-toolbar-top',
  templateUrl: './toolbar-top.component.html',
  styleUrls: ['./toolbar-top.component.scss']
})
export class ToolbarTopComponent {

  private viewModeTypeModel = ViewModeType;

  constructor(private eventData: EventDataService) { }

}
