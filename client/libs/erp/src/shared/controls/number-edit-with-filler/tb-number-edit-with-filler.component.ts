import { TbComponentService, LayoutService, EventDataService, ControlComponent } from '@taskbuilder/core';
import { Component, Input, ChangeDetectorRef } from '@angular/core';
import * as u from '../../../core/u/helpers';

@Component({
  selector: 'tb-number-edit-with-filler',
  templateUrl: './tb-number-edit-with-filler.component.html',
  styleUrls: ['./tb-number-edit-with-filler.component.scss']
})
export class NumberEditWithFillerComponent extends ControlComponent {
  @Input() enableFilling = true;
  @Input() maxLength = 20;
  @Input() fillerDigit = '0';
  @Input() minLen = 6;

  public errorMessage: string;

  constructor( public eventData: EventDataService,
    layoutService: LayoutService,
    tbComponentService: TbComponentService, 
    changeDetectorRef:ChangeDetectorRef) {
      super(layoutService, tbComponentService, changeDetectorRef);
     }

  onPasting(e: ClipboardEvent) {
    if (!u.ClipboardEventHelper.isNumber(e) ) {
      this.errorMessage = this._TB('Only numbers admitted.');
      e.preventDefault();
    }
  }

  onTyping(e: KeyboardEvent) {
    if (!u.KeyboardEventHelper.isNumber(e) )
      e.preventDefault();
  }

  changeModelValue(value) {
    this.model.value = value;
  }

  onBlur() {
    if (this.model.value.length === 0 || this.model.value.length >= this.maxLength) {
      this.blur.emit(this);
      this.eventData.change.emit(this.cmpId);
      return;
    }

    if (this.enableFilling && this.model.value.length < this.minLen) {
      let filler = '';
      for (let i = 0; i < (this.minLen - this.model.value.length); i++) {
        filler = filler + this.fillerDigit;
      }

      this.model.value = filler + this.model.value;
      this.model.value = this.model.value.substring(0, this.maxLength);
      this.blur.emit(this);
      this.eventData.change.emit(this.cmpId);
    }
  }
}
