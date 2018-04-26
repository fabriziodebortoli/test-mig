import { CheckStatus } from './../../../../models/check_status.enum';
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
  private _checkStatus = CheckStatus.UNDEFINED;
  @Input() caption: string = '--unknown--';
  @Input() cmpId: string = '';
  @Input() icon: string = '';

  @Input() b = false;
  constructor(public eventData: EventDataService, public ciService: ComponentInfoService) { }


  @Input() public set disabled(value: boolean) {
    this._disabled = value;
  }
  public get disabled(): boolean {
    if (this._disabled) {
      return true;
    }
    if (this.eventData.buttonsState &&
      this.eventData.buttonsState[this.cmpId])
      return !this.eventData.buttonsState[this.cmpId].enabled;
    return false;
  }
 
 @Input() public set checkStatus(value: CheckStatus) {
    this._checkStatus = value;
  }

  public get checkStatus(): CheckStatus {
    if (this._checkStatus != CheckStatus.UNDEFINED) {
      return this._checkStatus;
    }
    let status = undefined;
    if (this.eventData.buttonsState &&
      this.eventData.buttonsState[this.cmpId]) {
      status = this.eventData.buttonsState[this.cmpId].checkStatus;
    }
    return status as CheckStatus;
  }

  onCommand() {
    this.eventData.raiseCommand(this.ciService.getComponentId(), this.cmpId);
  }
}