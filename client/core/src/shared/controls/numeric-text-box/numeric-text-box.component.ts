import { ControlContainerComponent } from './../control-container/control-container.component';
import { TbComponentService } from './../../../core/services/tbcomponent.service';
import { LayoutService } from './../../../core/services/layout.service';
import { EventDataService } from './../../../core/services/eventdata.service';
import { FormattersService } from './../../../core/services/formatters.service';
import { Store } from './../../../core/services/store.service';

import { Component, Input, OnChanges, AfterViewInit, ChangeDetectorRef, ViewChild, SimpleChanges } from '@angular/core';

import { ControlComponent } from './../control.component';

import { Subscription } from '../../../rxjs.imports';

@Component({
  selector: 'tb-numeric-text-box',
  templateUrl: './numeric-text-box.component.html',
  styleUrls: ['./numeric-text-box.component.scss']
})
export class NumericTextBoxComponent extends ControlComponent implements OnChanges, AfterViewInit {
  @Input() forCmpID: string;
  @Input() disabled: boolean;

  // automatically generated by tbjson.exe, representing properties customization for numeric controls in tbjson files
  @Input() decimals = 0;
  // TB Constants
  @Input() minValue: number = 2.2250738585072014e-308;
  @Input() maxValue: number = 1.7976931348623158e+308;

  formatterProps: any;
  controlDecimals = 0;
  oldValue: number = null;
  editing = false;

  @ViewChild(ControlContainerComponent) cc: ControlContainerComponent;

  public constraint: RegExp = new RegExp('\\d');
  showError = '';
  public selectedValue: number;
  private stateButtonDisabled = false;
  private invertState = false;
  private stateModel: string = null;
  private modelChangedSubscription: Subscription;

  constructor(
    public eventData: EventDataService,
    private formattersService: FormattersService,
    private store: Store,
    layoutService: LayoutService,
    tbComponentService: TbComponentService,
    changeDetectorRef: ChangeDetectorRef
  ) {
    super(layoutService, tbComponentService, changeDetectorRef);
  }

  ngOnInit() {
    if (this.formatter)
      this.formatterProps = this.formattersService.getFormatter(this.formatter);
  }

  onStateInfo(event: any) {
    this.invertState = event.invertState;
    this.stateModel = event.model;
    this.store.select({
      'dataSource': event.model + '.value',
    }).subscribe(
      v => {
        if (v && v.dataSource !== undefined)
          // XNOR
          this.disabled = (this.invertState === v.dataSource);
        if (this.oldValue !== null && this.disabled) {
          this.selectedValue = this.oldValue;
          this.model.value = this.oldValue;
        }
      }
    );
  }

  getDecimals() {
    if (this.decimals > 0) {
      this.controlDecimals = this.decimals;
    } else {
      switch (this.formatter) {
        case 'FiscalYear':
        case 'Integer':
          this.controlDecimals = 0;
        default:
          if (this.formatterProps && this.formatterProps.refDecNumber) {
            this.controlDecimals = +this.formatterProps.refDecNumber;
          }
      }
    }
    return this.controlDecimals;
  }

  getMinValue(): any {
    switch (this.formatter) {
      case 'PositiveMoney':
      case 'PositiveRoundedMoney':
      case 'Fixing':
      case 'MoneyWithoutDecimal':
        return this.minValue && this.minValue > 0 ? this.minValue : 0;

      default:
        return this.minValue;
    }
  }

  getFormatOptions(): any {

    if (this.decimals > 0) {
      return 'n' + this.getDecimals().toString();
    } else {
      switch (this.formatter) {
        case 'FiscalYear':
        case 'Integer':
          return '#';
        default:
          return 'n' + this.getDecimals().toString();
      }
    }
  }

  ngAfterViewInit(): void {
    super.ngAfterViewInit();
    
    if (this.modelValid()) {
      this.selectedValue = this.model.value;
    }
  }

  onNgModelChange(value: number) {
    if (value == null)
      this.selectedValue = 0;
    this.model.value = value ? value : 0;
  }
  ngOnChanges(changes: SimpleChanges): void {
    if (this.modelValid()) {
      this.selectedValue = this.model.value;

      if (!this.modelChangedSubscription) {
        this.modelChangedSubscription = this.model.modelChanged.subscribe(
          () => {
            this.selectedValue = this.model.value;
            if (this.stateModel && !this.editing)
              this.oldValue = this.model.value;
            this.editing = false;
          }
        )
      }
    }
  }

  modelValid() {
    return this.model !== undefined && this.model !== null;
  }

  onBlur(e): any {
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
      this.cc.errorMessage = 'Input not in correct form';
      this.showError = 'inputError';
    }
    else {
      this.cc.errorMessage = '';
      this.showError = '';
    }
    this.editing = true;
    this.eventData.change.emit(this.cmpId);
    this.blur.emit(this);
  }
}
