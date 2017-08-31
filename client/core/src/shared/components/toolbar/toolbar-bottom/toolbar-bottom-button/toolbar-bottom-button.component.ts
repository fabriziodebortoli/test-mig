import { ComponentInfoService } from './../../../../../core/services/component-info.service';
import { Component, OnInit, Input } from '@angular/core';

import { EventDataService } from './../../../../../core/services/eventdata.service';

@Component({
  selector: 'tb-toolbar-bottom-button',
  templateUrl: './toolbar-bottom-button.component.html',
  styleUrls: ['./toolbar-bottom-button.component.scss']
})
export class ToolbarBottomButtonComponent implements OnInit {

  @Input() caption: string = '--unknown--';
  @Input() cmpId: string = '';

  @Input() disabled: boolean = false;

  constructor(private eventData: EventDataService, private ciService: ComponentInfoService) { }

  ngOnInit() {
  }

  onCommand() {
    this.eventData.raiseCommand(this.ciService.getComponentId(), this.cmpId);
  }
}