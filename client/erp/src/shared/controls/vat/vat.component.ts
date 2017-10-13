import { TbComponentService } from '@TaskBuilder/core/core/services/tbcomponent.service';
import { LayoutService } from '@TaskBuilder/core/core/services/layout.service';
import { EventDataService } from '@TaskBuilder/core/core/services/eventdata.service';
import { ControlComponent } from '@TaskBuilder/core/shared/controls';
import { Component, Input } from '@angular/core';
import { Store } from 'core/services';
import Tax from './tax';

@Component({
  selector: 'tb-vat',
  templateUrl: './vat.component.html',
  styleUrls: ['./vat.component.scss']
})
export class VatComponent extends ControlComponent {
  @Input('readonly') readonly = false;
  @Input() isoCode: string;
  errorMessage: any;

  constructor(
    private eventData: EventDataService,
    layoutService: LayoutService,
    tbComponentService: TbComponentService,
    store: Store
  ) {
    super(layoutService, tbComponentService);
    store
      .select(s => s[this.isoCode])
      .subscribe(s => this.validate());
  }

  onBlur() {
    this.validate();
    if (!this.isValid) return;
    this.blur.emit(this);
    this.eventData.change.emit(this.cmpId);
  }

  validate() {
    if (Tax.isValid(this.isoCode, this.model.value))
      this.errorMessage = this._TB("Vat code is not valid");
  }

  get isValid(): boolean { return !this.errorMessage; }
}
