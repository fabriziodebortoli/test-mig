import { Component, Input } from '@angular/core';
import { ControlComponent } from '@taskbuilder/core';
import { EventDataService } from '@taskbuilder/core';
import { TbComponentService } from '@taskbuilder/core';
import { LayoutService } from '@taskbuilder/core';
import { Store } from '@taskbuilder/core';

@Component({
  selector: 'erp-no-spaces',
  templateUrl: './no-spaces.component.html',
  styleUrls: ['./no-spaces.component.scss']
})
export class NoSpacesEditComponent extends ControlComponent {
  @Input('readonly') readonly: boolean = false;
  @Input() slice: any;
  @Input() selector: any;

  errorMessage = '';
  maxLength = 10;

  constructor(
    public eventData: EventDataService,
    layoutService: LayoutService,
    tbComponentService: TbComponentService,
    private store: Store
  ) {
    super(layoutService, tbComponentService);
  }
  ngOnInit() {
    // maxLength is an optional parameter, i may not have to use it.
    // It is also the only parameter, so i have no selector without it
    if (this.selector) {
      this.store
        .select(this.selector)
        .select('maxLength')
        .subscribe(
        (v) => {
          if (v)
            this.maxLength = v;
        }
        );
    }
  }

  onKeyDown($event) {
    if ($event.keyCode === 32) {
      $event.preventDefault();
    }
  }

  removeSpaces() {
    if (this.model && this.model.value)
      this.model.value = this.model.value.replace(/\s+/g, '');

    if (this.maxLength > 0 && this.model.value.length !== this.maxLength)
      this.errorMessage = 'Value length must be of ' + this.maxLength + ' chars';
    // this.errorMessage = this._TB('Value must be {0} chars', this.maxLength); Versione definitiva, dopo le modifiche di MArco
    else
      this.errorMessage = '';

    this.eventData.change.emit(this.cmpId);
    this.blur.emit(this);
  }
}
