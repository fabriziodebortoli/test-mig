import { ComponentInfoService } from './../../../../../core/services/component-info.service';
import { Component, OnInit, Input } from '@angular/core';

import { EventDataService } from './../../../../../core/services/eventdata.service';

@Component({
  selector: 'tb-toolbar-bottom-button',
  templateUrl: './toolbar-bottom-button.component.html',
  styleUrls: ['./toolbar-bottom-button.component.scss']
})
export class ToolbarBottomButtonComponent {

  @Input() caption: string = '--unknown--';
  @Input() cmpId: string = '';
  @Input() disabled: boolean = false;
  @Input() icon:string = '';

  constructor(public eventData: EventDataService, public ciService: ComponentInfoService) { }


  isDisabled(): boolean {
    return false;
    // return this.disabled ||
    // !this.eventData.buttonsState || 
    // !this.eventData.buttonsState[this.cmpId] || 
    // !this.eventData.buttonsState[this.cmpId].enabled;
  }
  onCommand() {
    this.eventData.raiseCommand(this.ciService.getComponentId(), this.cmpId);
  }
}