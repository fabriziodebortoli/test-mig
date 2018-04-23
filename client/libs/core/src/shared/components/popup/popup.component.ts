import { Component, Input, Output, EventEmitter, ViewChild, ChangeDetectionStrategy } from '@angular/core';
import { EventDataService } from './../../../core/services/eventdata.service';

@Component({
    selector: 'tb-popup',
    styleUrls: ['./popup.component.scss'],
    templateUrl: './popup.component.html'
})
export class PopupComponent {

    @Input() class: string = '';
    @Input() anchor:string;
    @Input() icon:string;
    @Input() cmpId:string;
    private _disabled = false;

    constructor( public eventData: EventDataService) {
    }

    
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
}