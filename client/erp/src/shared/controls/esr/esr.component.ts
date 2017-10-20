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
      if (!Helpers.hasBeenTypedANumber(e) )
        e.preventDefault();
    }

     ngOnChanges(changes) {
        this.validate();
    }

     onBlur() {
        this.validate();
        if (this.isValid) {
        }
     }

     validate() {
        if (!this.model) return;
        if (!Esr.checkEsrDigit(this.value))
          this.errorMessage = this._TB('Incorrect ESR Check Digit, value expected {0} value found {1}.', );
      }
    
      get isValid(): boolean { return !this.errorMessage; }
}
