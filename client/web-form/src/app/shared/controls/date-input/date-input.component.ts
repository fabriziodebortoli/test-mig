import { Component, Input, ViewChild, OnChanges, AfterViewInit } from '@angular/core';
import { ControlComponent } from './../control.component';
import { Align } from '@progress/kendo-angular-popup/dist/es/models/align.interface';
import * as moment from 'moment';

@Component({
  selector: 'tb-date-input',
  templateUrl: './date-input.component.html',
  styleUrls: ['./date-input.component.scss']
})

export class DateInputComponent extends ControlComponent implements OnChanges, AfterViewInit {
  @Input() forCmpID: string;
  @ViewChild('kendoMaskedTextBoxInstance') kendoMasked: any;
  anchorAlign: Align = { horizontal: 'right', vertical: 'bottom' };
  popupAlign: Align = { horizontal: 'left', vertical: 'center' };

  singlePlaceHolder = '_';
  placeHolder = '__';
  separator = '/';
  defaultValue = this.placeHolder + this.separator + this.placeHolder + this.separator + this.placeHolder + this.placeHolder;
  ngValue = this.defaultValue;

  selectedDate: Date;
  popupOpen = false;
  showValidationResult = false;
  doubleEvent = false;
  validationResult = '';
  dateFormat = 'DD/MM/YYYY';

  public mask = 'dA/mA/YAAA';
  invalidDateString = 'Invalid Date';
  public rules: { [key: string]: RegExp } = {
    'A': /[0-9]/,
    'd': /[0-3]/,
    'm': /[01]/,
    'Y': /[12]/
  };

  public handleChange(value: Date): void {
    this.onUpdateNgModel(value);
    this.onClickIconCalend();
    this.resetParamsValidation();
  }

  onClickIconCalend(): void {
    this.popupOpen = !this.popupOpen;
    this.doubleEvent = false;
  }

  onBlur(): void {
    this.popupOpen = this.doubleEvent;
  }

  private press(): void { // necessario per evitare che sul ckick di chiusura, il blur annulli il click
    this.doubleEvent = true;
  }

  onUpdateNgModel(newDate: Date): void {
    this.selectedDate = newDate;  // aggiornare la data selezionata nel calendario in apertura
    this.ngValue = moment.parseZone(this.selectedDate, this.dateFormat).format(this.dateFormat);
    this.setModelValueDB();
  }

  setModelValueDB(): void {
    if (this.model === null) {
      this.model = { enable: 'true', value: '' };
    }
    this.model.value = moment.parseZone(this.ngValue, this.dateFormat).toDate().toJSON().substring(0, 19);
    console.log('this.model.value = ' + this.model.value);
  }

  ngAfterViewInit(): void {
    if (this.model !== undefined && this.model !== null) {
      this.onUpdateNgModel(new Date(this.model.value));
    }
  }

  ngOnChanges(): void {
    if (this.model !== undefined && this.model !== null) {
      this.onUpdateNgModel(new Date(this.model.value));
    }
  }

  resetParamsValidation(setTrue: boolean = false): void {
    this.validationResult = setTrue ? this.invalidDateString : '';
    this.showValidationResult = setTrue;
    this.kendoMasked.maskValidation = setTrue;
  }

  validateDate(): void {
    if (this.ngValue === this.defaultValue) { return; }

    this.resetParamsValidation(true);

    let formattedDate = this.formatDate(); // se ci sono '_', li sostituisce con zeri
    if (formattedDate === null || !formattedDate.isValid()) { return; }

    this.showValidationResult = false;
    this.onUpdateNgModel(formattedDate.toDate());
  }

  formatDate(): moment.Moment {
    if (this.ngValue.search(this.singlePlaceHolder) === -1) {
      return moment(this.ngValue, this.dateFormat);
    }

    let arrayDate: string[] = this.ngValue.split(this.separator);
    for (let index = 0; index < arrayDate.length; index++) {
      if (arrayDate[index] === this.placeHolder || arrayDate[index] === this.placeHolder + this.placeHolder) {
        return null;
      }
      while (arrayDate[index].search(this.singlePlaceHolder) !== -1) {
        arrayDate[index] = arrayDate[index].replace(this.singlePlaceHolder, '0');
      }
    }
    this.ngValue = '';
    for (let index = 0; index < arrayDate.length; index++) {
      this.ngValue += arrayDate[index];
    }
    this.ngValue = moment.parseZone(this.ngValue, this.dateFormat).format(this.dateFormat);
    return moment(this.ngValue, this.dateFormat);
  }

}
