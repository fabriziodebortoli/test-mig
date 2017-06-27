import { Component, Input } from '@angular/core';

import { EventDataService } from '@taskbuilder/core';
import { ViewModeType } from '../../';

@Component({
  selector: 'tb-toolbar-top',
  templateUrl: './toolbar-top.component.html',
  styleUrls: ['./toolbar-top.component.scss']
})
export class ToolbarTopComponent {

  @Input() title: string = '...';

  private viewModeTypeModel = ViewModeType;

  constructor(private eventData: EventDataService) { }

}
