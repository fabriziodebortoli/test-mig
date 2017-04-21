import { Component, Input } from '@angular/core';
import { ControlComponent } from './../control.component';


@Component({
  selector: 'tb-numeric-text-box',
  templateUrl: './numeric-text-box.component.html',
  styleUrls: ['./numeric-text-box.component.scss']
})
export class NumericTextBoxComponent extends ControlComponent {
@Input() forCmpID: string;
@Input() formatter: string;
@Input() disabled: boolean;

public formatOption: any = {
  minimumFractionDigits: 2,
  maximumFractionDigits: 3,
  useGrouping: true
};



getFormatOptions(formatter: string): string {
    switch (formatter) {
      case 'Integer':
        this.formatOption = {
        style: 'decimal',
        useGrouping: false
};
        break;
      default: break;
    }
    return this.formatOption;
  }
}
