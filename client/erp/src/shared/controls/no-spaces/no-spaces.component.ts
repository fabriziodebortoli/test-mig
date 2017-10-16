import { Component, Input } from '@angular/core';
import { ControlComponent } from '@TaskBuilder/core/shared/controls';
import { EventDataService } from '@TaskBuilder/core/core/services/eventdata.service';
import { TbComponentService } from '@TaskBuilder/core/core/services/tbcomponent.service';
import { LayoutService } from '@TaskBuilder/core/core/services/layout.service';

@Component({
  selector: 'no-spaces',
  templateUrl: './no-spaces.component.html',
  styleUrls: ['./no-spaces.component.css']
})
export class NoSpacesEditComponent extends ControlComponent {
  @Input('readonly') readonly: boolean = false;
  @Input() public maxLength = 0;
  errorMessage = '';

  constructor(
    public eventData: EventDataService,
    layoutService: LayoutService,
    tbComponentService: TbComponentService
  ) {
    super(layoutService, tbComponentService);
  }

  onKeyDown($event) {
    if ($event.keyCode === 32) {
      $event.preventDefault();
    }
  }

  removeSpaces() {
    if (this.model && this.model.value)
      this.model.value = this.model.value.replace(/\s+/g, '');

    if (this.maxLength > 0 && this.model.value.length > this.maxLength)
      this.errorMessage = 'Value must be max ' + this.maxLength + ' chars';
    else
      this.errorMessage = '';

    this.eventData.change.emit(this.cmpId);
    this.blur.emit(this);
  }
}
