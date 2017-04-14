import { ControlComponent } from './../control.component';
import { Component, Input } from '@angular/core';
import { EventDataService } from './../../../core/eventdata.service';

@Component({
  selector: 'tb-email',
  templateUrl: './email.component.html',
  styleUrls: ['./email.component.scss']
})

export class EmailComponent extends ControlComponent {
   @Input('readonly') readonly: boolean = false;
  constructor(private eventData: EventDataService) {
    super();
  }

   onBlur() {
    this.eventData.change.emit(this.cmpId);
  }
}
