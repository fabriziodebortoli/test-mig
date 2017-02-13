import { Component, Input } from '@angular/core';
import { ViewModeType } from '../../';

@Component({
  selector: 'tb-toolbar-top',
  templateUrl: './toolbar-top.component.html',
  styleUrls: ['./toolbar-top.component.scss']
})

export class ToolbarTopComponent {

  @Input('title') title: string = '...';

  @Input('viewModeType') viewModeType: ViewModeType = ViewModeType.D;
  viewModeTypeModel = ViewModeType;

  constructor() { }

}
