import { Component, Input } from '@angular/core';

import { EventDataService } from './../../../services/eventdata.service';
import { ViewModeType } from '../../../../shared';

@Component({
  selector: 'tb-toolbar-top',
  template: "<div class=\"toolbar-top\"> <div class=\"toolbar-top-item title\"> <span class=\"document-title truncate\">{{eventData?.model?.Title?.value || title}}</span> </div> <div class=\"toolbar-top-item functions\"></div> <div class=\"toolbar-top-item navigation\"> <ng-content></ng-content> </div> </div>",
  styles: [".toolbar-top { display: flex; flex-direction: row; flex-wrap: nowrap; justify-content: space-between; align-items: stretch; background: #fff; border-bottom: 1px solid #ddd; height: 30px; } .title { flex-grow: 2; display: flex; } .navigation { display: flex; } .document-title { line-height: 32px; margin: 0 10px; font-weight: 700; font-size: 14px; text-transform: uppercase; } "]
})
export class ToolbarTopComponent {

  @Input() title: string = '...';

  private viewModeTypeModel = ViewModeType;

  constructor(private eventData: EventDataService) { }

}
