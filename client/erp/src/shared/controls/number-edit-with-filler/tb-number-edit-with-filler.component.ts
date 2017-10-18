import { TbComponentService, LayoutService, EventDataService, ControlComponent } from '@taskbuilder/core';
import { Component, Input } from '@angular/core';

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
  
  private regex: RegExp = new RegExp(/^-?\d+$/);
  private admittedSpecialKeys: Array<string> = [ 'Backspace', 'Tab', 'End', 'Home', 'ArrowLeft', 'ArrowRight', 'Escape'];

  constructor( private eventData: EventDataService,
    layoutService: LayoutService,
    tbComponentService: TbComponentService) {
      super(layoutService, tbComponentService);
     }

  onPaste(event) {
    const e = <ClipboardEvent> event;
    if (e.type === 'paste') {
      const pasteData = e.clipboardData.getData('text');
      if (pasteData && !String(pasteData).match(this.regex)) {
          e.preventDefault();
          this.errorMessage = this._TB('Only integers admitted.');
      }
    }
  }

  onKeyDown(event) {
    this.errorMessage = '';
    const e = <KeyboardEvent> event;
    if (this.admittedSpecialKeys.indexOf(e.key) !== -1) {
      return;
    }
    if (e.ctrlKey === true && (e.key === 'c' || e.key === 'x' || e.key === 'v' || e.key === 'z')) {
        return;
    }

    const current: string = event.key;
    const next: string = current.concat(e.key);
    if (next && !String(next).match(this.regex)) {
      e.preventDefault();
    }

    if (this.model.value.length >= this.maxLength) {
      e.preventDefault();
    }
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
