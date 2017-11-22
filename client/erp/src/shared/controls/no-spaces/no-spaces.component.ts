import { Component, Input,ChangeDetectorRef } from '@angular/core';
import { ControlComponent } from '@taskbuilder/core';
import { EventDataService } from '@taskbuilder/core';
import { TbComponentService } from '@taskbuilder/core';
import { LayoutService } from '@taskbuilder/core';

@Component({
  selector: 'erp-no-spaces',
  templateUrl: './no-spaces.component.html',
  styleUrls: ['./no-spaces.component.scss']
})
export class NoSpacesEditComponent extends ControlComponent {
  @Input('readonly') readonly: boolean = false;
  @Input() slice: any;
  errorMessage = '';
  maxLength = 10;

  constructor(
    public eventData: EventDataService,
    layoutService: LayoutService,
    tbComponentService: TbComponentService,
    changeDetectorRef:ChangeDetectorRef
  ) {
    super(layoutService, tbComponentService, changeDetectorRef);
  }
  ngOnChanges(changes) {

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
