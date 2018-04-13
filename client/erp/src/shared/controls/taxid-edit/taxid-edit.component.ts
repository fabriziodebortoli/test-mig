import { FormMode, ContextMenuItem, Store, TbComponentService, LayoutService, ControlComponent, ActivationService, EventDataService, ControlContainerComponent, createSelector } from '@taskbuilder/core';
import { DataService, HttpService, ParameterService, MessageDlgResult, MessageDlgArgs, Selector } from '@taskbuilder/core';
import { Component, Input, ChangeDetectorRef, OnInit, ViewChild } from '@angular/core';
import { CoreHttpService } from '../../../core/services/core/core-http.service';
import { Http, Headers, RequestOptions, Response, URLSearchParams } from '@angular/http';
import * as moment from 'moment';
import JsCheckTaxId from './jscheckTaxIDNumber';
import { untilDestroy } from '@taskbuilder/core/shared/commons/untilDestroy';

@Component({
  selector: 'erp-taxid-edit',
  templateUrl: './taxid-edit.component.html',
  styleUrls: ['./taxid-edit.component.scss']
})
export class TaxIdEditComponent extends ControlComponent implements OnInit {
  @Input('readonly') readonly = false;
  @Input() slice;
  @Input() selector: Selector<any, any>;
  @Input('maxLength') maxLength: number = 524288;
  @Input('textLimit') textlimit: number = 0;

  @ViewChild(ControlContainerComponent) cc: ControlContainerComponent;

  private ctrlEnabled = false;
  private isMasterBR = false;
  private isMasterIT = false;
  private isMasterRO = false;
  private isMasterES = false;
  private isEuropeanUnion = false;
  private naturalPerson = false;
  private isoCode = '';
  private taxIdType = 0;

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
    private activationService: ActivationService,
    private http: Http) {
    super(layoutService, tbComponentService, changeDetectorRef);
  }

  ngOnInit() {
    if (this.selector) {
        this.store.select(
          createSelector(
          this.selector.nest('naturalPerson.value'),
          this.selector.nest('isoCode.value'),
          this.selector.nest('taxIdType.value'),
          s => s.FormMode && s.FormMode.value,
          t => t.BatchMode && t.BatchMode.value,
          (naturalperson, isocode, taxIdType, formMode, batchMode) => ({ naturalperson, isocode, taxIdType, formMode, batchMode })
        )
      )
        .pipe(untilDestroy(this))
        .subscribe(this.onSelectorChanged);
    }
    else {
      console.log("Missing selector in " + this.cmpId);
      //this.cc.errorMessage = 'Missing selector';
    }

    this.store.select(_ => this.model && this.model.length).
    subscribe(l => this.onlenghtChanged(l));

    this.isMasterBR = this.activationService.isActivated('ERP', 'MasterData_BR');
    this.isMasterIT = this.activationService.isActivated('ERP', 'MasterData_IT');
    this.isMasterRO = this.activationService.isActivated('ERP', 'MasterData_RO');
    this.isMasterES = this.activationService.isActivated('ERP', 'MasterData_ES');
    this.isEuropeanUnion = this.activationService.isActivated('ERP', 'EuropeanUnion');
  }
  
  onlenghtChanged(len: any) {
    if (len !== undefined)
      this.setlength(len);
  }

  public openMessageDialog(message: string): Promise<any> {
    let args = new MessageDlgArgs();
    this.eventData.openMessageDialog.emit({ ...args, yes: true, no: true, text: message });
    return this.eventData.closeMessageDialog.take(1).toPromise();
  }

  onSelectorChanged = obj => {
    this.ctrlEnabled = obj.formMode === FormMode.FIND || obj.formMode === FormMode.NEW || obj.formMode === FormMode.EDIT || obj.batchMode === true;
    this.naturalPerson = obj.naturalperson;
    let isoChanged = this.isoCode != obj.isocode;
    let taxIdTypeChanged = this.taxIdType != obj.taxIdType;
    this.isoCode = obj.isocode;
    this.taxIdType = obj.taxIdType;

    this.buildContextMenu();

    if (!this.ctrlEnabled) {
      this.cc.errorMessage = '';
      this.changeDetectorRef.detectChanges();
    }

    if (isoChanged || taxIdTypeChanged)
      this.validate();
  }

  buildContextMenu() {
    this.cc.contextMenu.splice(0, this.cc.contextMenu.length);

    if (!this.ctrlEnabled) return;

    if (this.isTaxIdField(this.model.value, false))
      this.cc.contextMenu.push(this.menuItemITCheck);

    if (this.isMasterRO && this.isoCode === 'RO' && !this.naturalPerson && this.isTaxIdField(this.model.value, true))
      this.cc.contextMenu.push(this.menuItemROCheck);

    if (this.isEuropeanUnion && this.isTaxIdField(this.model.value, true))
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
      this.changeDetectorRef.detectChanges();
      return;
    }

    try {
      let r = await this.httpCore.checkVatRO(this.model.value, today).toPromise();
      let result = r.json();
      if (result.found.length) {
        this.cc.errorMessage = this._TB('VALID: The Tax code or Fiscal code is correct.');
        this.fillFields(result.found);
      } else {
        if (!result.ok)
          this.cc.errorMessage = result.statusCode;
        else
          this.cc.errorMessage = this._TB('INVALID: Incorrect Tax code or fiscal code.');
      }
    } catch (exc) {
      this.cc.errorMessage = exc;
    }
    this.changeDetectorRef.detectChanges();
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

  // async prova()
  // {
  //   let p = new URLSearchParams();
  //   p.set('filter', 'OLT');
  //   p.set('ISOcode', 'RO');
  //   p.set('disabled', '0');

  //   let data = await this.dataService.getData('ERP.Company.Dbl.CountiesQuery', 'direct', p).take(1).toPromise();
  // }

  async fillFields(result: any) {
    let slice = await this.store.select(this.selector).take(1).toPromise();
    if (slice.companyName) {
      let company = result[0].denumire;
      let exTemp = result[0].tva;

      let reg = /(MUN.\s+|MUNICIPUL\s+|MUNICIPIUL\s+|JUD.\s+|JUDETUL\s+|)/g;
      let splitAddress = (<string>result[0].adresa).replace(reg, '').split(',');
      let description = splitAddress[0];

      let p = new URLSearchParams();
      p.set('filter', description);
      p.set('ISOcode', this.isoCode);
      p.set('disabled', '0');

      let data = await this.dataService.getData('ERP.Company.Dbl.CountiesQuery', 'direct', p).take(1).toPromise();
      let county: any;
      if (data !== undefined) {
        county = data.rows[0].MA_Counties_County;
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

    getTaxIdTypeString(taxIdType: any): string {
        if (Number(taxIdType) === 33226752)
            return this._TB("Tax Number");
        else if (Number(taxIdType) === 33226753)
            return this._TB("Tax Number Intra");
        else if (Number(taxIdType) === 33226754)
            return this._TB("Passport");
        else if (Number(taxIdType) === 33226755)
            return this._TB("Official Document in the Country");
        else if (Number(taxIdType) === 33226756)
            return this._TB("Residence Certificate");
        else if (Number(taxIdType) === 33226757)
            return this._TB("Other Document");
        else
            return this._TB("");
    }

    getSpanishTaxIdType(taxIdNumber: any, taxIdType: any): any {
        // controlla solo congruit� tra taxIdNumber (corretto) e il taxIdType
        var bIsNaturalPerson = false;
        var bIsForeignNaturalPerson = false;
        var bIsJuridicalPerson = false;
        var vatexp = new Array();
        vatexp.push(/^([0-9])/);
        vatexp.push(/^([LKXYZM])/);

        if (vatexp[0].test(taxIdNumber[0]))
            bIsNaturalPerson = true;
        if (vatexp[1].test(taxIdNumber[0]))
            bIsForeignNaturalPerson = true;
        bIsJuridicalPerson = !bIsNaturalPerson && !bIsForeignNaturalPerson;
	    // - se si tratta di NIE allora deve essere selezionato il tipo Residenza Certificata
	    // - se NON si tratta di NIE allora NON deve essere selezionato il tipo Residenza Certificata
        return (bIsForeignNaturalPerson ? 33226756 : 33226752);
    }

    setErrorSpanishTaxIdNumber(kind: any, taxIdType: any) {
        if (Number(kind) === 7733248)
            this.cc.errorMessage = this._TB('The entered Document is a {0}. Enter a correct one or change the Document Type.', this.getTaxIdTypeString(taxIdType));
        else
            this.cc.errorMessage = this._TB('The entered Document is not a {0}. Enter a correct one or change the Document Type.', this.getTaxIdTypeString(taxIdType));
    }


  async validate() {
    this.cc.errorMessage = '';
    if (!this.model || !this.ctrlEnabled || !this.value || this.value === '') return;

    let slice = await this.store.select(this.selector).take(1).toPromise();
    // per Spagna
      if (this.isMasterES) {
        if (this.isoCode === 'ES' && !slice.taxIdType)//in loc. Spagna se manca taxIdType e isoCode Spagna non pu� controllare
            return;              
        if (this.isoCode === 'ES' &&
            (Number(slice.taxIdType) !== 33226752 &&
            Number(slice.taxIdType) !== 33226753 &&
            Number(slice.taxIdType) !== 33226756))
            return;
        if (this.isoCode !== 'ES' &&
            Number(slice.taxIdType) !== 33226753)
            return;
    }
      if (!JsCheckTaxId.isTaxIdValid(this.model.value, this.isoCode)) {
          if (this.isMasterES && this.isoCode === 'ES')
              this.setErrorSpanishTaxIdNumber(slice.kind, slice.taxIdType);
          else
              this.cc.errorMessage = this._TB('Incorrect Tax Number');
      }
      else if (this.isMasterES && this.isoCode === 'ES') {
          // se partita iva corretta ma siamo in loc. Spagna e isoCode � Spagna
          // viene controllato che il taxIdType sia congruente
          let taxIdTypeCalc = this.getSpanishTaxIdType(this.model.value, slice.taxIdType);
          if (Number(taxIdTypeCalc) !== Number(slice.taxIdType))
              this.setErrorSpanishTaxIdNumber(slice.kind, taxIdTypeCalc);
      }

    this.changeDetectorRef.detectChanges();
  }

  isTaxIdNumber(fiscalcode: string): boolean {
    if (!fiscalcode) {
      return false;
    }
    return fiscalcode.charAt(0) >= '0' && fiscalcode.charAt(0) <= '9';
  }

  get isValid(): boolean { return !this.cc.errorMessage; }

  setlength(len: number) {
    if (len > 0) 
      this.maxLength = len;
    if (this.textlimit > 0 && (this.maxLength == 0 || this.textlimit < this.maxLength)) {
      this.maxLength = this.textlimit;
    }
  }
}
