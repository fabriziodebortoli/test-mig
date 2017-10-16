import { TbComponentService, LayoutService, ControlComponent, EventDataService } from '@taskbuilder/core';
import { Component, Input } from '@angular/core';
import Tax from './tax';

@Component({
  selector: 'erp-vat',
  templateUrl: './vat.component.html',
  styleUrls: ['./vat.component.scss']
})
export class VatComponent extends ControlComponent {
  @Input('readonly') readonly = false;
  @Input() isoCode: string;
  errorMessage: any;

  constructor( private eventData: EventDataService, layoutService: LayoutService, tbComponentService: TbComponentService ) {
    super(layoutService, tbComponentService);
  }

  ngOnChanges(changes) {
    this.validate();
  }

  onBlur() {
    this.validate();
    if (!this.isValid) return;
    this.blur.emit(this);
    this.eventData.change.emit(this.cmpId);
  }

  validate() {
    if (Tax.isValid(this.isoCode, this.model.value))
      this.errorMessage = this._TB('Vat code is not valid');
  }

  get isValid(): boolean { return !this.errorMessage; }
}
