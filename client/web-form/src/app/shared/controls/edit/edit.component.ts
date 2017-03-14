import { StateButton } from './../state-button/state-button.model';
import { EventDataService } from './../../../core/eventdata.service';
import { ControlComponent } from './../control.component';
import { Component, Input } from '@angular/core';
import {ControlTypes} from '../control-types.enum';

@Component({
    selector: 'tb-edit',
    templateUrl: 'edit.component.html',
    styleUrls: ['./edit.component.scss']
})

export class EditComponent extends ControlComponent{
    @Input() buttons: StateButton[] = [];
    @Input() controlType: ControlTypes;
    controlTypeModel = ControlTypes;
     constructor(
        private eventData: EventDataService
      ) {
        super();
      }

    onBlur() {
        this.eventData.change.emit(this.cmpId);
    }
}
