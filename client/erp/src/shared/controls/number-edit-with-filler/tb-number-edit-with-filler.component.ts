import { TbComponentService, LayoutService, EventDataService, ControlComponent } from '@taskbuilder/core';
import { Component, Input } from '@angular/core';
import { Helpers } from '../../../core/u/helpers';

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
  public value: any;

  constructor( public eventData: EventDataService,
    layoutService: LayoutService,
    tbComponentService: TbComponentService) {
      super(layoutService, tbComponentService);
     }

  onPasting(e: ClipboardEvent) {
    if (!Helpers.hasBeenPastedANumber(e) ) {
      this.errorMessage = this._TB('Only numbers admitted.');
      e.preventDefault();
    }
  }

  onTyping(e: KeyboardEvent) {
    if (!Helpers.hasBeenTypedANumber(e) )
      e.preventDefault();
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
