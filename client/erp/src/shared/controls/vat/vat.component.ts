import { Logger, Store, TbComponentService, LayoutService, ControlComponent, EventDataService } from '@taskbuilder/core';
import { Component, Input, ChangeDetectorRef } from '@angular/core';
import { ErpHttpService } from '../../../core/services/erp-http.service';
import JsVat from './jsvat';

@Component({
  selector: 'erp-vat',
  templateUrl: './vat.component.html',
  styleUrls: ['./vat.component.scss']
})
export class VatComponent extends ControlComponent {
  @Input('readonly') readonly = false;
  @Input() isoCode: string;
  @Input() slice;
  @Input() selector;
  errorMessage: any;

  constructor(layoutService: LayoutService, 
    private eventData: EventDataService, 
    private logger: Logger,
    tbComponentService: TbComponentService, 
    private http: ErpHttpService, 
    changeDetectorRef:ChangeDetectorRef,
    private store: Store) {
    super(layoutService, tbComponentService, changeDetectorRef);
  }

  ngOnChanges(changes) {
    this.validate();
    if (changes.slice) {
      this.logger.debug('SLICE CHANGED ' + (changes.slice.currentValue && changes.slice.currentValue.propertyChangedName) +
        '\n' + JSON.stringify(changes.slice));
    }
  }

  async onBlur() {
    this.validate();
    if (!this.isValid) return;
    let r = await this.http.isVatDuplicate(this.model.value).toPromise();
    if (r.json().isDuplicate)
      this.errorMessage = r.json().message;
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
    if (!JsVat.isTaxIdValid(this.model.value, this.isoCode))
      this.errorMessage = this._TB('Vat code is not valid');
  }

  get isValid(): boolean { return !this.errorMessage; }
}
