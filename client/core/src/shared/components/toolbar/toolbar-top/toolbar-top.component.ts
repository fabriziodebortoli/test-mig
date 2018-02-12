import { Component, Input } from '@angular/core';

import { ViewModeType } from './../../../../shared/models/view-mode-type.model';

import { EventDataService } from './../../../../core/services/eventdata.service';

@Component({
  selector: 'tb-toolbar-top',
  templateUrl: './toolbar-top.component.html',
  styleUrls: ['./toolbar-top.component.scss']
})
export class ToolbarTopComponent {

  @Input() caption: string = '...';
  @Input() history: boolean = false;

  public viewModeTypeModel = ViewModeType;

  constructor(public eventData: EventDataService) { }

}
