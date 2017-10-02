import { Component, Input } from '@angular/core';

import { ViewModeType } from './../../../../shared/models';

import { EventDataService } from './../../../../core/services/eventdata.service';

@Component({
  selector: 'tb-toolbar-top',
  templateUrl: './toolbar-top.component.html',
  styleUrls: ['./toolbar-top.component.scss']
})
export class ToolbarTopComponent {

  @Input() title: string = '...';

  public viewModeTypeModel = ViewModeType;

  constructor(public eventData: EventDataService) { }

}
