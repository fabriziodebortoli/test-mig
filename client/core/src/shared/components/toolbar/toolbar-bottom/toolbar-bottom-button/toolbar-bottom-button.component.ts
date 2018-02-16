import { ComponentInfoService } from './../../../../../core/services/component-info.service';
import { Component, OnInit, Input } from '@angular/core';

import { EventDataService } from './../../../../../core/services/eventdata.service';

@Component({
  selector: 'tb-toolbar-bottom-button',
  templateUrl: './toolbar-bottom-button.component.html',
  styleUrls: ['./toolbar-bottom-button.component.scss']
})
export class ToolbarBottomButtonComponent {

  private _disabled = false;
  @Input() caption: string = '--unknown--';
  @Input() cmpId: string = '';
  @Input() icon:string = '';

  constructor(public eventData: EventDataService, public ciService: ComponentInfoService) { }


  @Input() public set disabled(value: boolean) {
    this._disabled = value;
  }
  public get disabled(): boolean {
    return this._disabled ||
      (this.eventData.buttonsState &&
      this.eventData.buttonsState[this.cmpId] &&
      !this.eventData.buttonsState[this.cmpId].enabled);
  }
  onCommand() {
    this.eventData.raiseCommand(this.ciService.getComponentId(), this.cmpId);
  }
}