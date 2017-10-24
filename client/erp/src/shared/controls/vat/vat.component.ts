import { TbComponentService, LayoutService, ControlComponent, EventDataService } from '@taskbuilder/core';
import { Component, Input } from '@angular/core';
import { ErpHttpService } from '../../../core/services/erp-http.service';
import Tax from './tax';

@Component({
  selector: 'erp-vat',
  templateUrl: './vat.component.html',
  styleUrls: ['./vat.component.scss']
})
export class VatComponent extends ControlComponent {
  @Input('readonly') readonly = false;
  @Input() isoCode: string;
  @Input() slice;

  errorMessage: any;

  constructor(layoutService: LayoutService, private eventData: EventDataService, 
    tbComponentService: TbComponentService, private http: ErpHttpService) {
    super(layoutService, tbComponentService);
  }

  ngOnChanges(changes) {
    this.validate();
    if (changes.slice)
      console.log('b: ' + JSON.stringify(changes.slice));
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
    if (!Tax.isValid(this.isoCode, this.model.value))
      this.errorMessage = this._TB('Vat code is not valid');
  }

  get isValid(): boolean { return !this.errorMessage; }
}
