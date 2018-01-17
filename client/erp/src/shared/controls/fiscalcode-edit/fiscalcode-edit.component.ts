import { Logger, Store, TbComponentService, LayoutService, ControlComponent, EventDataService } from '@taskbuilder/core';
import { Component, Input, ChangeDetectorRef } from '@angular/core';
import { CoreHttpService } from '../../../core/services/core/core-http.service';
import JsCheckTaxId from '../taxid-edit/jscheckTaxIDNumber';

@Component({
  selector: 'erp-fiscalcode-edit',
  templateUrl: './fiscalcode-edit.component.html',
  styleUrls: ['./fiscalcode-edit.component.scss']
})
export class FiscalCodeEditComponent extends ControlComponent {
  @Input('readonly') readonly = false;
  @Input() isoCode: string;
  @Input() slice;
  @Input() selector;
  errorMessage: any;

  constructor(layoutService: LayoutService,
    private eventData: EventDataService,
    private logger: Logger,
    tbComponentService: TbComponentService,
    private http: CoreHttpService,
    changeDetectorRef: ChangeDetectorRef,
    private store: Store) {
    super(layoutService, tbComponentService, changeDetectorRef);
  }

  ngOnChanges(changes) {
    if (changes.slice && changes.slice.isoCode) {
      this.isoCode = changes.slice.isoCode.value;
    }
    this.validate();
  }

  async onBlur() {
    this.validate();
    if (!this.isValid) return;
    this.blur.emit(this);
    this.eventData.change.emit(this.cmpId);
  }

  changeModelValue(value) {
    this.model.value = value;
    this.validate();
  }

  validate() {
    this.errorMessage = '';
    if (!this.model) return;

    this.isFiscalCodeValid(this.model.value, this.isoCode);

  }

  isTaxIdNumber(fiscalcode: string): boolean {
    if (!fiscalcode) {
      return false;
    }
    return fiscalcode.charAt(0) >= '0' && fiscalcode.charAt(0) <= '9';
  }

  isFiscalCodeValid(fiscalcode: string, country: string) {
    if (!this.isSupported(country)) {
      this.errorMessage = '';
      return;
    }

    if (!fiscalcode) {
      this.errorMessage = '';
      return;
    }

    switch (country) {
      case 'IT':
        this.FiscalCodeCheckIT(fiscalcode);
        break;
      case 'BR':
        this.FiscalCodeCheckBR(fiscalcode);
        break;
      case 'ES':
        this.FiscalCodeCheckES(fiscalcode);
        break;
    }
  }

  isSupported(country: string): boolean {
    if (!country) {
      return false;
    }

    const supported = [
      'IT',
      'BR',
      'ES'];

    return !!supported.find(c => c === country.toUpperCase());
  }

  FiscalCodeCheckIT(fiscalcode: string) {
    if (this.isTaxIdNumber(fiscalcode)) {
      if (!JsCheckTaxId.isTaxIdValid(fiscalcode, this.isoCode))
        this.errorMessage = this._TB('Incorrect fiscal code number');
      return;
    }

    if (fiscalcode.length != 16) {
      this.errorMessage = this._TB('The fiscal code number must have at least 16 characters!');
      return;
    }
    const regex = /[A-Za-z]{6}[0-9]{2}[A-Za-z][0-9]{2}[A-Za-z][0-9]{3}[A-Za-z]/g;
    // const str = `RGLMRA65M16D969N`;
    let m;

    if ((m = regex.exec(fiscalcode)) === null) {
      this.errorMessage = this._TB("The fiscal code number entered does not comply with the structure: AAA-AAA-NN-A-NN-ANNN-A");
      return;
    }

    const tbc = [
      0,
      1, 0, 5, 7, 9,
      13, 15, 17, 19, 21,
      2, 4, 18, 20, 11,
      3, 6, 8, 12, 14,
      16, 10, 22, 25, 24,
      23, 1, 0, 5, 7,
      9, 13, 15, 17, 19,
      21
    ];

    let array = Array.from(fiscalcode).map(c => c.toUpperCase().charCodeAt(0));

    let nOdd = 0;
    for (let i = 0; i <= 14; i += 2) {
      let n = array[i] - (array[i] < 58 ? 21 : 64);
      nOdd += tbc[n];
    }

    let nEqual = 0;
    for (let i = 1; i <= 13; i += 2) {
      nEqual += array[i] - (array[i] < 58 ? 48 : 65);
    }

    let check = (nOdd + nEqual) % 26 + 65;

    if (check != array[15])
      this.errorMessage = this._TB('Incorrect fiscal code number');

  }

  FiscalCodeCheckBR(fiscalcode: string) {
    if (fiscalcode.length != 14) {
      this.errorMessage = this._TB('The fiscal code number must have at least 14 characters!');
      return;
    }
    const regex = /([0-9]{3}[\.]){2}[0-9]{3}[-][0-9]{2}/g;
    // const str = `123.456.789-12`;
    let m;

    if ((m = regex.exec(fiscalcode)) === null) {
      this.errorMessage = this._TB("The fiscal code number entered does not comply with the structure: NNN.NNN.NNN-NN");
      return;
    }

    let array = Array.from(fiscalcode.replace(/[.]/g, '').replace(/[-]/g, '')).map(c => c.charCodeAt(0) - 48);

    let lSum = 0;
    for (let i = 0; i < 9; i++) {
      lSum += array[8 - i] * (i + 2);
    }
    let nCtrlDigit1 = 11 - lSum % 11;
    if (nCtrlDigit1 > 9) nCtrlDigit1 = 0;

    lSum = 0;
    for (let i = 0; i < 10; i++) {
      lSum += array[9 - i] * (i + 2);
    }
    let nCtrlDigit2 = 11 - lSum % 11;
    if (nCtrlDigit2 > 9) nCtrlDigit2 = 0;

    if (nCtrlDigit1 != array[9] || nCtrlDigit2 != array[10])
      this.errorMessage = this._TB('Incorrect fiscal code number');
  }


  FiscalCodeCheckES(fiscalcode: string) {
    if (fiscalcode.length != 9) {
      this.errorMessage = this._TB('The fiscal code number must have 9 characters!');
      return;
    }

    const regex = /[A-Za-z][0-9]{7}[A-Za-z]/g;
    // const str = `A1234567B`;
    let m;

    if ((m = regex.exec(fiscalcode)) === null) {
      this.errorMessage = this._TB("The fiscal code number entered does not comply with the structure: X9999999X");
      return;
    }
  }

  get isValid(): boolean { return !this.errorMessage; }
}
