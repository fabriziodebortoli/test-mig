import { TbComponentService } from './../../../core/services/tbcomponent.service';
import { LayoutService } from './../../../core/services/layout.service';
import { EventDataService } from './../../../core/services/eventdata.service';
import { FormattersService } from './../../../core/services/formatters.service';

import { Component, Input, OnChanges, AfterViewInit, ChangeDetectorRef } from '@angular/core';

import { ControlComponent } from './../control.component';

import { align } from '@progress/kendo-drawing/main';

@Component({
  selector: 'tb-numeric-text-box',
  templateUrl: './numeric-text-box.component.html',
  styleUrls: ['./numeric-text-box.component.scss']
})
export class NumericTextBoxComponent extends ControlComponent implements OnChanges, AfterViewInit {
  @Input() forCmpID: string;
  @Input() disabled: boolean;
  @Input() formatterDecimals = 0;
  @Input() public hotLink: any = undefined;

  formatter: any;
  decimals = 0;
  errorMessage: string;
  public constraint: RegExp = new RegExp('\\d');
  showError = '';
  public selectedValue: number;

  // public formatOptionsCurrency: any = {
  //   style: 'currency',
  //   currency: 'EUR'/*,
  //   currencyDisplay: 'name'*/
  // };
  // public formatOptionsInteger: any = {
  //   style: 'decimal'
  // };

  // public formatOptionsDouble = 'F';

  // public formatOptionsPercent: any = {
  //   style: 'percent'
  // };

  constructor(
    public eventData: EventDataService,
    private formattersService: FormattersService,
    layoutService: LayoutService,
    tbComponentService: TbComponentService,
    changeDetectorRef: ChangeDetectorRef
  ) {
    super(layoutService, tbComponentService, changeDetectorRef);

    // DEBUG
    this.format = "Double";
  }

  ngOnInit() {
    if (this.format)
      this.formatter = this.formattersService.getFormatter(this.format);
  }

  getDecimals() {
    if (this.formatterDecimals > 0) {
      this.decimals = this.formatterDecimals;
    } else {
      switch (this.format) {
        case 'FiscalYear':
          this.decimals = 0;
        default:
          if (this.formatter && this.formatter.refDecNumber) {
            this.decimals = +this.formatter.refDecNumber;
          }
      }
    }
    return this.decimals;
  }

  getMinValue(): any {
    switch (this.format) {
      case 'PositiveMoney':
      case 'PositiveRoundedMoney':
      case 'Fixing':
      case 'MoneyWithoutDecimal':
        return 0;

      default:
        return null;
    }
  }

  getFormatOptions(): any {
    switch (this.format) {
      case 'Percent':
        return 'p';
      default:
        return 'n' + this.getDecimals().toString();
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
        break;
      case 'Double':
        this.constraint = new RegExp('[-+]?[0-9]*\.?[0-9]+');
        break;

      default: break;
    }

    if (!this.constraint.test(this.model.value)) {
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
