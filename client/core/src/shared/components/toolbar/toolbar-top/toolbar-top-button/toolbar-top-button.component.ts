import { CheckStatus } from './../../../../models/check_status.enum';
import { Component, Input, ChangeDetectorRef } from '@angular/core';

import { ComponentInfoService } from './../../../../../core/services/component-info.service';
import { TbComponentService } from './../../../../../core/services/tbcomponent.service';
import { InfoService } from './../../../../../core/services/info.service';
import { EventDataService } from './../../../../../core/services/eventdata.service';

import { TbComponent } from '../../../tb.component';

@Component({
  selector: 'tb-toolbar-top-button',
  templateUrl: './toolbar-top-button.component.html',
  styleUrls: ['./toolbar-top-button.component.scss']
})
export class ToolbarTopButtonComponent extends TbComponent {

  private _disabled = false;
  private _checkStatus = CheckStatus.UNDEFINED;
  @Input() caption: string = '';
  @Input() iconType: string = 'M4'; // MD, TB, CLASS, IMG  
  @Input() _icon: string = '';

  @Input()
  set icon(icon: any) {
    this._icon = icon instanceof Object ? icon.value : icon;
  }

  get icon() {
    return this._icon;
  }

  /**
   * Optional command called on button click.
   * @returns return true to call default command as well.
   */
  @Input() click: () => boolean = () => true;

  imgUrl: string;

  constructor(
    public eventData: EventDataService,
    public infoService: InfoService,
    public ciService: ComponentInfoService,
    public tbComponentService: TbComponentService,
    changeDetectorRef: ChangeDetectorRef
  ) {
    super(tbComponentService, changeDetectorRef);
  }
  onCommand() {
    if (!this.click())
      return;
    this.eventData.raiseCommand(this.ciService.getComponentId(), this.cmpId);
  }

  @Input() public set disabled(value: boolean) {
    this._disabled = value;
  }
  public get disabled(): boolean {
    return this._disabled ||
      (this.eventData.buttonsState &&
        this.eventData.buttonsState[this.cmpId] &&
        !this.eventData.buttonsState[this.cmpId].enabled);
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
    return status ? status : CheckStatus.UNDEFINED;
  }
}