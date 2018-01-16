import { Logger, Store, TbComponentService, LayoutService, ControlComponent, EventDataService } from '@taskbuilder/core';
import { Component, Input, ChangeDetectorRef } from '@angular/core';
import { CoreHttpService } from '../../../core/services/core/core-http.service';
import JsCheckTaxId from './jscheckTaxIDNumber';

@Component({
  selector: 'erp-taxid-edit',
  templateUrl: './taxid-edit.component.html',
  styleUrls: ['./taxid-edit.component.scss']
})
export class TaxIdEditComponent extends ControlComponent {
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

  // ngAfterContentInit() {
  //   this.subscribeToSelector();
  // }

  // subscribeToSelector() {
  //   if (this.store && this.selector) {
  //     this.store
  //       .select(this.selector)
  //       .select('isoCode')
  //       .subscribe(
  //       (v) => this.onIsoCodeChanged(v)
  //       );
  //   }
  // }

  // onIsoCodeChanged(isocode: any) {
  //   if (isocode == undefined)
  //     return;

  //   this.isoCode = isocode.value;
  //   this.validate();
  // }

  ngOnChanges(changes) {
    if (changes.slice && changes.slice.isoCode) {
      this.isoCode = changes.slice.isoCode.value;
      // this.logger.debug('SLICE CHANGED ' + (changes.slice.currentValue && changes.slice.currentValue.propertyChangedName) +
      //   '\n' + JSON.stringify(changes.slice));
    }
    this.validate();
  }

  async onBlur() {
    //let slice = await this.store.select(this.selector).take(1).toPromise();
    this.validate();
    if (!this.isValid) return;
    // let r = await this.http.isVatDuplicate(this.model.value).toPromise();
    // if (r.json().isDuplicate)
    //   this.errorMessage = r.json().message;
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
    if (!JsCheckTaxId.isTaxIdValid(this.model.value, this.isoCode))
      this.errorMessage = this._TB('Incorrect Tax Number');
  }

  get isValid(): boolean { return !this.errorMessage; }
}
