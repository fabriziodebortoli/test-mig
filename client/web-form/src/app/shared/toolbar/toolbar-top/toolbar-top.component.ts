import { ViewModeType } from '../../';
import { EventDataService } from '../../../core';
import { Component, Input } from '@angular/core';

@Component({
  selector: 'tb-toolbar-top',
  templateUrl: './toolbar-top.component.html',
  styleUrls: ['./toolbar-top.component.scss']
})
export class ToolbarTopComponent {

  private viewModeTypeModel = ViewModeType;

  constructor(private eventData: EventDataService) { }

}
