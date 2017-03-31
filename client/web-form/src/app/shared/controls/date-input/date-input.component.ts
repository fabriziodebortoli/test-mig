import { Component, Input, ViewChild, OnChanges } from '@angular/core';
import { ControlComponent } from './../control.component';
import { Align } from "@progress/kendo-angular-popup/dist/es/models/align.interface";

@Component({
  selector: 'tb-date-input',
  templateUrl: './date-input.component.html',
  styleUrls: ['./date-input.component.scss']
})

export class DateInputComponent extends ControlComponent implements OnChanges {
  @Input() forCmpID: string;
  @ViewChild('kendoMaskedTextBoxInstance') kendoMasked: any;
  anchorAlign: Align = { horizontal: 'right', vertical: 'bottom' };
  popupAlign: Align = { horizontal: 'left', vertical: 'center' };

  placeHolder = '__';
  separator = '/';
  defaultValue = this.placeHolder + this.separator + this.placeHolder + this.separator + this.placeHolder + this.placeHolder;
  modelValue = this.defaultValue;

  selectedDate: Date;
  switchP = false;
  showValidationResult = false;
  doubleEvent = false;
  validationResult = '';

  public mask = 'dA/mA/YAAA';
  invalidDateString = 'Invalid Date';
  public rules: { [key: string]: RegExp } = {
    'A': /[0-9]/,
    'd': /[0-3]/,
    'm': /[01]/,
    'Y': /[12]/
  };

  public handleChange(value: Date): void {
    this.onUpdateModel(value);
    this.onClickM();
    this.resetParams();
  }

  onClickM(): void {
    this.switchP = !this.switchP;
    this.doubleEvent = false;
  }

  onBlur(): void {
    if (! this.doubleEvent) 
      this.switchP = false;
  }

  private press(): void { // necessario per evitare che sul ckick di chiusura, il blur annulli il click
    this.doubleEvent = true;
  }

  onUpdateModel(newDate: Date): void {
    this.selectedDate = newDate;
    this.modelValue = this.selectedDate.toLocaleDateString('en-GB');
    this.onSave();
  }

  onSave(): void {
    if (this.selectedDate === undefined || this.model === null) { return; }
    let y = new Date(this.selectedDate.getFullYear(), this.selectedDate.getMonth(), this.selectedDate.getDate(),
      12, this.selectedDate.getMinutes(), this.selectedDate.getSeconds());
    this.model.value = y.toJSON().substring(0, 19);
    console.log('this.model.value = ' + this.model.value);
  }

  ngAfterViewInit(): void {
    if (this.model !== undefined && this.model !== null) {
      this.onUpdateModel(new Date(this.model.value));
    }
  }

  ngOnChanges(): void {
    if (this.model !== undefined && this.model !== null) {
      this.onUpdateModel(new Date(this.model.value));
    }
  }

  resetParams(setTrue:boolean = false): void {
    this.validationResult = setTrue ? this.invalidDateString : '';
    this.showValidationResult = setTrue;
    this.kendoMasked.maskValidation = setTrue;
  }

  validateDate(): void {
    if (this.modelValue === this.defaultValue)   return;

    this.resetParams(true);

    let formattedDate: Date = this.formatDate();
    if(formattedDate === null) return;

    if (Object.prototype.toString.call(formattedDate) !== '[object Date]')
      return;
    if (!isNaN(formattedDate.getTime())) {    // date is valid
        this.selectedDate = formattedDate;
        this.showValidationResult = false;
        this.onUpdateModel(formattedDate);
    }
    this.validationResult = formattedDate.toLocaleDateString('en-GB');

  }

  formatDate(): Date {
    let arrayDate: string[] = this.modelValue.split(this.separator);
    for (let index = 0; index < arrayDate.length; index++) 
    {
      let element = arrayDate[index];
      if(element === this.placeHolder || element === this.placeHolder + this.placeHolder) 
        return null;
      if(element.search('_') !== -1)
        arrayDate[index] = element.replace('_', '0');    
    }
    return new Date(arrayDate[2] + this.separator + arrayDate[1] + this.separator + arrayDate[0] + ' 12:00:00');
  }

}
