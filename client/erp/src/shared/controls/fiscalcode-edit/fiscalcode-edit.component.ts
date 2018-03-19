import { FormMode, ContextMenuItem, Store, TbComponentService, LayoutService, ControlComponent, EventDataService, 
  ActivationService, ParameterService, ControlContainerComponent, Selector } from '@taskbuilder/core';
import { untilDestroy } from '@taskbuilder/core/shared/commons/untilDestroy';
import { Component, Input, ChangeDetectorRef, OnInit, OnChanges, ViewChild } from '@angular/core';
import { CoreHttpService } from '../../../core/services/core/core-http.service';
import { Http, Headers } from '@angular/http';

import JsCheckTaxId from '../taxid-edit/jscheckTaxIDNumber';

@Component({
  selector: 'erp-fiscalcode-edit',
  templateUrl: './fiscalcode-edit.component.html',
  styleUrls: ['./fiscalcode-edit.component.scss']
})
export class FiscalCodeEditComponent extends ControlComponent implements OnInit {
  @Input('readonly') readonly = false;
  @Input() slice;
  @Input() selector: Selector<any, any>;

  @ViewChild(ControlContainerComponent) cc: ControlContainerComponent;

  private ctrlEnabled = false;
  private isMasterBR = false;
  private isMasterIT = false;
  private isEuropeanUnion = false;
  private isoCode = '';

  menuItemITCheck = new ContextMenuItem(this._TB('Check TaxId existence (IT site)'), '', true, false, null, this.checkIT.bind(this));
  menuItemEUCheck = new ContextMenuItem(this._TB('Check TaxId existence (EU site)'), '', true, false, null, this.checkEU.bind(this));
  menuItemBRCheck = new ContextMenuItem(this._TB('Check Fiscal Code existence'), '', true, false, null, this.checkBR.bind(this));

  constructor(layoutService: LayoutService,
    private eventData: EventDataService,
    tbComponentService: TbComponentService,
    changeDetectorRef: ChangeDetectorRef,
    private parameterService: ParameterService,
    private store: Store,
    private activationService: ActivationService,
    private httpCore: CoreHttpService,
    private http: Http) {
    super(layoutService, tbComponentService, changeDetectorRef);
  }

  async ngOnInit() {
    this.store
      .select(s => s && s.FormMode.value)
      .pipe(untilDestroy(this))
      .subscribe(v =>
        this.onFormModeChanged(v));

    if (this.selector) {
      this.store
        .select(this.selector.nest('isoCode.value'))
        .pipe(untilDestroy(this))
        .subscribe(this.onIsoCodeChanged);
    }
    else {
      console.log("Missing selector in " + this.cmpId );
      //this.cc.errorMessage = 'Missing selector';
    }

    this.isMasterBR = this.activationService.isActivated('ERP', 'MasterData_BR');
    this.isMasterIT = this.activationService.isActivated('ERP', 'MasterData_IT');
    this.isEuropeanUnion = this.activationService.isActivated('ERP', 'EuropeanUnion');
    // this.httpservice.isActivated('ERP', 'MasterData_BR').take(1).subscribe(res => { this.isMasterBR = res.result; })
    // this.httpservice.isActivated('ERP', 'MasterData_IT').take(1).subscribe(res => { this.isMasterIT = res.result; })
    // this.httpservice.isActivated('ERP', 'EuropeanUnion').take(1).subscribe(res => { this.isEuropeanUnion = res.result; })
  }

  onFormModeChanged(formMode: FormMode) {
    this.ctrlEnabled = formMode === FormMode.FIND || formMode === FormMode.NEW || formMode === FormMode.EDIT;
    this.buildContextMenu();

    if (!this.ctrlEnabled) {
      this.cc.errorMessage = '';
      this.changeDetectorRef.detectChanges();
    }
  }

  onIsoCodeChanged = (isoCode: string) => {
    this.isoCode = isoCode;
    this.validate();
  }

  buildContextMenu() {
    this.cc.contextMenu.splice(0, this.cc.contextMenu.length);

    if (!this.ctrlEnabled) return;

    if (this.isTaxIdField(this.model.value, false))
      this.cc.contextMenu.push(this.menuItemITCheck);

    if (this.isEuropeanUnion && this.isTaxIdField(this.model.value, false))
      this.cc.contextMenu.push(this.menuItemEUCheck);

    if (this.isMasterBR && this.isoCode === 'BR')
      this.cc.contextMenu.push(this.menuItemBRCheck);
  }

  isTaxIdField(code: string, all: boolean) {
    return code !== '' && (all || this.isMasterIT);
  }

  async onBlur() {
    this.buildContextMenu();
    this.validate();
    if (!this.isValid) return;
    this.blur.emit(this);
    this.eventData.change.emit(this.cmpId);
  }

  changeModelValue(value) {
    this.model.value = value;
  }

  async checkIT() {
    let stato = await this.getStato();

    let url = `http://www1.agenziaentrate.it/servizi/vies/vies.htm?act=piva&s=${stato}&p=${this.model.value}`;
    var newWindow = window.open(url, 'blank');
  }

  async checkBR() {
    //let url ='https://www.receita.fazenda.gov.br/pessoajuridica/cnpj/cnpjreva/cnpjreva_solicitacao.asp';
    let url = 'https://www.receita.fazenda.gov.br/Aplicacoes/SSL/ATCTA/CPF/ConsultaSituacao/ConsultaPublica.asp';
    var newWindow = window.open(url, 'blank');
  }

  async checkEU() {
    let stato = await this.getStato();

    let r = await this.httpCore.checkVatEU(stato, this.model.value).toPromise();
    if (r.json().isValid)
      this.cc.errorMessage = this._TB('VALID: The Tax code or Fiscal code is correct.');
    else
      this.cc.errorMessage = this._TB('INVALID: Incorrect Tax code or fiscal code.');

    this.changeDetectorRef.detectChanges();
  }

  async getStato(): Promise<string> {
    let stato = this.isoCode;
    if (stato == '' || stato == undefined) {
      stato = await this.parameterService.getParameter('MA_CustSuppParameters.ISOCountryCode');
    }

    if (stato == '' || stato == undefined)
      stato = 'IT'

    return stato;
  }

  async validate() {
    if (this.ctrlEnabled)
      this.isFiscalCodeValid(this.model.value);
  }

  isTaxIdNumber(fiscalcode: string): boolean {
    if (!fiscalcode) {
      return false;
    }
    return fiscalcode.charAt(0) >= '0' && fiscalcode.charAt(0) <= '9';
  }

  async isFiscalCodeValid(fiscalcode: string) {

    this.cc.errorMessage = '';
    if (!this.model || !fiscalcode) return;

    let stato = await this.getStato();

    if (!this.isSupported(stato)) {
      return;
    }

    switch (stato) {
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
    this.changeDetectorRef.detectChanges();
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
      if (!JsCheckTaxId.isTaxIdValid(fiscalcode, 'IT'))
        this.cc.errorMessage = this._TB('Incorrect fiscal code number');
      return;
    }

    if (fiscalcode.length != 16) {
      this.cc.errorMessage = this._TB('The fiscal code number must have at least 16 characters!');
      return;
    }
    const regex = /[A-Za-z]{6}[0-9]{2}[A-Za-z][0-9]{2}[A-Za-z][0-9]{3}[A-Za-z]/g;
    let m;

    if ((m = regex.exec(fiscalcode)) === null) {
      this.cc.errorMessage = this._TB("The fiscal code number entered does not comply with the structure: AAA-AAA-NN-A-NN-ANNN-A");
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
      this.cc.errorMessage = this._TB('Incorrect fiscal code number');

  }

  FiscalCodeCheckBR(fiscalcode: string) {
    if (fiscalcode.length != 14) {
      this.cc.errorMessage = this._TB('The fiscal code number must have at least 14 characters!');
      return;
    }
    const regex = /([0-9]{3}[\.]){2}[0-9]{3}[-][0-9]{2}/g;
    // const str = `123.456.789-12`;
    let m;

    if ((m = regex.exec(fiscalcode)) === null) {
      this.cc.errorMessage = this._TB("The fiscal code number entered does not comply with the structure: NNN.NNN.NNN-NN");
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
      this.cc.errorMessage = this._TB('Incorrect fiscal code number');
  }

  FiscalCodeCheckES(fiscalcode: string) {
    if (fiscalcode.length != 9) {
      this.cc.errorMessage = this._TB('The fiscal code number must have 9 characters!');
      return;
    }

    const regex = /[A-Za-z][0-9]{7}[A-Za-z]/g;
    // const str = `A1234567B`;
    let m;

    if ((m = regex.exec(fiscalcode)) === null) {
      this.cc.errorMessage = this._TB("The fiscal code number entered does not comply with the structure: X9999999X");
      return;
    }
  }

  get isValid(): boolean { return !this.cc.errorMessage; }
}
