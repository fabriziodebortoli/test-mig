import { EventDataService } from './../../../core/eventdata.service';
import { Component, Input, OnChanges, AfterViewInit} from '@angular/core';
import { ControlComponent } from './../control.component';
  

@Component({
  selector: 'tb-numeric-text-box',
  templateUrl: './numeric-text-box.component.html',
  styleUrls: ['./numeric-text-box.component.scss']
})
export class NumericTextBoxComponent extends ControlComponent implements OnChanges, AfterViewInit {
@Input() forCmpID: string;
@Input() formatter: string;
@Input() disabled: boolean;
@Input() decimals: number;
@Input() width: number;
private errorMessage: string;
private constraint: RegExp =  new RegExp('\\d');
public selectedValue: number;
private showError = '';



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

 constructor(private eventData: EventDataService) {
  super();
}

 getDecimalsOptions(): number {
     switch (this.formatter) {
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
       return this.formatOptionsInteger ;

      case 'Double':
        return this.formatOptionsDouble;

        case 'Money':
        return this.formatOptionsCurrency;

       case 'Percent':
       return this.formatOptionsPercent;
     default: break;
    }
  }


public onChange(val: any) {
    this.onUpdateNgModel(val);
  }

  onUpdateNgModel(newValue: number): void {
    if (!this.modelValid()) {
      this.model = { enable: 'true', value: '' };
    }
    this.selectedValue = newValue;
    this.model.value = newValue;

  }

  ngAfterViewInit(): void {
    if (this.modelValid()) {
      this.onUpdateNgModel(this.model.value);
    }
  }

  ngOnChanges(): void {
    if (this.modelValid()) {
      this.onUpdateNgModel(this.model.value);
    }
  }

  modelValid() {
    return this.model !== undefined && this.model !== null;
  }


  onBlur(): any {
    switch (this.formatter) {
      case 'Integer':
      case 'Long':
      case 'Money':
      case 'Percent':
          this.constraint = new RegExp('\\d');
           break ;
      case 'Double':
         this.constraint = new RegExp('[-+]?[0-9]*\.?[0-9]+');
        break;

     default: break;
    }

     if (!this.constraint.test(this.model.value))
     {
       this.errorMessage = 'Input not in correct form';
        this.showError = 'inputError';
     }
       else {
         this.errorMessage = '';
        this.showError = '';
       }
    this.eventData.change.emit(this.cmpId);
    this.blur.emit(this);
  }
}
