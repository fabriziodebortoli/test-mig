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
@Input() decimals: number;
public formatOptionsCurrency: any = {
        style: 'currency',
        currency: 'EUR',
        currencyDisplay: 'name'
    };
public formatOptionsInteger: any = {
       style: 'decimal'
    };

public formatOptionsDouble = 'F';

public formatOptionsPercent: any = {
       style: 'percent'
    };

 public caseSwitch: string;
 getDecimalsOptions(formatter: string): number {
     switch (formatter) {
       case 'Integer':
       case 'Long':
       this.decimals = 0; break;

     case 'Money':
         this.decimals = 2; break;
       default: break;
     }
     return this.decimals;
   }


getFormatOptions(): any {
    switch (this.formatter) {
      case 'Integer':
       case 'Long':
       this.caseSwitch = '1';
       return this.formatOptionsInteger ;

      case 'Double':
        this.caseSwitch = '2';
        return this.formatOptionsDouble;

        case 'Money':
        this.caseSwitch = '3';
        return this.formatOptionsCurrency;

       case 'Percent':
       this.caseSwitch = '4';
       return this.formatOptionsPercent;
     default: break;
    }
  }
}
