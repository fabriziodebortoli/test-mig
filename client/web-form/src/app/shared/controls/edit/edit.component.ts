import { EventDataService } from './../../../core/eventdata.service';
import { ControlComponent } from './../control.component';
import { Component } from '@angular/core';


@Component({
    selector: 'tb-edit',
    templateUrl: 'edit.component.html',
    styleUrls: ['./edit.component.scss']
})

export class EditComponent extends ControlComponent{
    constructor(
        private eventData: EventDataService
      ) {
        super();
      }

    onBlur() {
        this.eventData.change.emit(this.cmpId);
    }
}
