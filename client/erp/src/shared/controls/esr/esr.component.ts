import { TbComponentService, LayoutService, EventDataService, ControlComponent } from '@taskbuilder/core';
import { Component, Input } from '@angular/core';
import Esr from './esr';
import { Helpers } from '../../../core/u/helpers';

@Component({
  selector: 'erp-esr',
  templateUrl: './esr.component.html',
  styleUrls: ['./esr.component.scss']
})
export class EsrComponent extends ControlComponent {

  @Input() minLen = 0;
  public errorMessage = '';

  constructor( public eventData: EventDataService,
    layoutService: LayoutService,
    tbComponentService: TbComponentService) {
      super(layoutService, tbComponentService);
      this.minLen = 0;
     }

     onPasting(e: ClipboardEvent) {
      if (!Helpers.hasBeenPastedANumber(e) ) {
        this.errorMessage = this._TB('Only numbers admitted.');
        e.preventDefault();
      }
    }

    onTyping(e: KeyboardEvent) {
      this.errorMessage = '';
      if (!Helpers.hasBeenTypedANumber(e) )
        e.preventDefault();
    }

    changeModelValue(value) {
      this.model.value = value;
      this.validate();
    }

    onBlur() {
        this.validate();
     }

     validate() {
        if (!this.model) return;
        this.errorMessage = '';
        let r = Esr.checkEsrDigit(this.model.value);
        if (!r.result) this.errorMessage = this._TB(r.error);
      }

      get isValid(): boolean { return !this.errorMessage; }
}