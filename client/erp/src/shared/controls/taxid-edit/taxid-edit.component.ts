import { FormMode, ContextMenuItem, Store, TbComponentService, LayoutService, ControlComponent, EventDataService, ControlContainerComponent } from '@taskbuilder/core';
import { DataService, HttpService, ParameterService, MessageDlgResult, MessageDlgArgs } from '@taskbuilder/core';
import { Component, Input, ChangeDetectorRef, OnInit, OnChanges, ViewChild } from '@angular/core';
import { CoreHttpService } from '../../../core/services/core/core-http.service';
import { Http, Headers, RequestOptions, Response, URLSearchParams } from '@angular/http';
import * as moment from 'moment';
import JsCheckTaxId from './jscheckTaxIDNumber';

@Component({
  selector: 'erp-taxid-edit',
  templateUrl: './taxid-edit.component.html',
  styleUrls: ['./taxid-edit.component.scss']
})
export class TaxIdEditComponent extends ControlComponent implements OnInit, OnChanges {
  @Input('readonly') readonly = false;
  @Input() slice;
  @Input() selector;
  
  @ViewChild(ControlContainerComponent) cc: ControlContainerComponent;

  private ctrlEnabled = false;
  private isMasterBR = false;
  private isMasterIT = false;
  private isMasterRO = false;
  private isEuropeanUnion = false;
  private naturalPerson = false;
  private isoCode = '';

  menuItemITCheck = new ContextMenuItem(this._TB('Check TaxId existence (IT site)'), '', true, false, null, this.checkIT.bind(this));
  menuItemEUCheck = new ContextMenuItem(this._TB('Check TaxId existence (EU site)'), '', true, false, null, this.checkEU.bind(this));
  menuItemBRCheck = new ContextMenuItem(this._TB('Check Fiscal Code existence'), '', true, false, null, this.checkBR.bind(this));
  menuItemROCheck = new ContextMenuItem(this._TB('Tax Status Check'), '', true, false, null, this.checkRO.bind(this));

  constructor(layoutService: LayoutService,
    private eventData: EventDataService,
    private dataService: DataService,
    tbComponentService: TbComponentService,
    changeDetectorRef: ChangeDetectorRef,
    private parameterService: ParameterService,
    private store: Store,
    private httpservice: HttpService,
    private httpCore: CoreHttpService,
    private http: Http) {
    super(layoutService, tbComponentService, changeDetectorRef);
  }

  ngOnChanges(changes) {
    if (changes.slice) {
      if (changes.slice.isoCode) {
        this.isoCode = changes.slice.isoCode.value;
        this.buildContextMenu();
      }
      if (changes.slice.naturalPerson) {
        this.naturalPerson = changes.slice.naturalPerson.value;
        this.buildContextMenu();
      }
    }
    this.validate();
  }

  ngOnInit() {
    this.store
      .select('FormMode.value')
      .subscribe(v =>
        this.onFormModeChanged(v.value));

    this.httpservice.isActivated('ERP', 'MasterData_BR').take(1).subscribe(res => { this.isMasterBR = res.result; })
    this.httpservice.isActivated('ERP', 'MasterData_IT').take(1).subscribe(res => { this.isMasterIT = res.result; })
    this.httpservice.isActivated('ERP', 'MasterData_RO').take(1).subscribe(res => { this.isMasterRO = res.result; })
    this.httpservice.isActivated('ERP', 'EuropeanUnion').take(1).subscribe(res => { this.isEuropeanUnion = res.result; })
  }

  public openMessageDialog(message: string): Promise<any> {
    let args = new MessageDlgArgs();
    this.eventData.openMessageDialog.emit({ ...args, yes: true, no: true, text: message });
    // this.eventData.closeMessageDialog.take(1).subscribe(s => console.log(s));
    return this.eventData.closeMessageDialog.take(1).toPromise();
  }

  onFormModeChanged(formMode: FormMode) {
    this.ctrlEnabled = formMode == FormMode.FIND || formMode == FormMode.NEW || formMode == FormMode.EDIT;
    this.buildContextMenu();
  }

  buildContextMenu() {
    this.cc.contextMenu.splice(0, this.cc.contextMenu.length);

    if (!this.ctrlEnabled) return;

    if (this.isTaxIdField(this.model.value, false))
    this.cc.contextMenu.push(this.menuItemITCheck);

    if (this.isMasterRO && this.isoCode === 'RO' && !this.naturalPerson && this.isTaxIdField(this.model.value, false))
    this.cc.contextMenu.push(this.menuItemROCheck);

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
    this.validate();
  }

  async checkIT() {
    let stato = await this.getStato();

    let url = `http://www1.agenziaentrate.it/servizi/vies/vies.htm?act=piva&s=${stato}&p=${this.model.value}`;
    let newWindow = window.open(url, 'blank');
  }

  async checkBR() {
    let url = 'https://www.receita.fazenda.gov.br/pessoajuridica/cnpj/cnpjreva/cnpjreva_solicitacao.asp';
    let newWindow = window.open(url, 'blank');
  }

  // '23260646'; taxid RO
  async checkRO() {
    let now = moment();
    let today = now.format('YYYY-MM-DD');
    let vatCode = this.model.value.replace(/([\D])/g, '');

    if (vatCode.length > 9 || vatCode.length === 0) {
      this.cc.errorMessage = this._TB('INVALID: Incorrect Tax code or fiscal code.');
      return;
    }

    try {
      let r = await this.httpCore.checkVatRO(this.model.value, today).toPromise();
      let found = r.json().found;
      if (found.length) {
        this.cc.errorMessage = this._TB('VALID: The Tax code or Fiscal code is correct.');
        this.fillFields(found);
      } else {
        this.cc.errorMessage = this._TB('INVALID: Incorrect Tax code or fiscal code.');
      }
    } catch (exc) {
      this.cc.errorMessage = exc;
    }
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

  async fillFields(result: any) {
    let slice = await this.store.select(this.selector).take(1).toPromise();
    if (slice.companyName) {
      let company = result[0].denumire;
      let exTemp = result[0].tva;

      let reg = /(MUN.\s+|MUNICIPUL\s+|MUNICIPIUL\s+|JUD.\s+|JUDETUL\s+|)/g;
      let splitAddress = (<string>result[0].adresa).replace(reg, '').split(',');

      let data = await this.dataService.getData('DataFile.ERP.Company.County', 'code', new URLSearchParams()).take(1).toPromise();
      let county: any;
      if (data !== undefined) {
        county = data.rows.find(x => x.Description === splitAddress[0]);
      }

      if (county === undefined)
        county = { County: '' };

      let city = splitAddress[1];
      let address = '';
      if (splitAddress[2] !== '' || splitAddress[3] !== '') {
        address = splitAddress[2];
        if (address !== '' && splitAddress[3] !== '') {
          address = address + ', ' + splitAddress[3];
        }
      }

      let taxExempt = exTemp === 'true' ? this._TB('Tax Exempt') : this._TB('Tax Subject');

      let message = this._TB('Tax Number found:\n{0}\n{1},{2},{3}\n{4}\nDo you want to overwrite data?',
        company, address, city, county.County, taxExempt);

      if ((await this.openMessageDialog(message)).yes) {
        if (slice.companyName) {
          slice.companyName.value = company;
        }

        if (slice.address) {
          slice.address.value = address;
        }

        if (slice.city) {
          slice.city.value = city;
        }

        if (slice.county) {
          slice.county.value = county.County;
        }
        this.changeDetectorRef.detectChanges();
      }
    }
  }

  async getStato(): Promise<string> {
    let stato = this.isoCode;
    if (stato === '' || stato === undefined) {
      stato = await this.parameterService.getParameter('MA_CustSuppParameters.ISOCountryCode');
    }

    if (stato === '' || stato === undefined)
      stato = 'IT'

    return stato;
  }

  validate() {
    this.cc.errorMessage = '';
    if (!this.model) return;
    if (!JsCheckTaxId.isTaxIdValid(this.model.value, this.isoCode))
      this.cc.errorMessage = this._TB('Incorrect Tax Number');
  }

  isTaxIdNumber(fiscalcode: string): boolean {
    if (!fiscalcode) {
      return false;
    }
    return fiscalcode.charAt(0) >= '0' && fiscalcode.charAt(0) <= '9';
  }

  get isValid(): boolean { return !this.cc.errorMessage; }
}
