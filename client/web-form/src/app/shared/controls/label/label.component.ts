import { EventDataService } from './../../../core/eventdata.service';
import { ControlComponent } from './../control.component';
import { Component } from '@angular/core';


@Component({
    selector: 'tb-label',
    templateUrl: 'label.component.html',
    styleUrls: ['./label.component.scss']
})

export class LabelComponent extends ControlComponent{

     constructor(
        private eventData: EventDataService
      ) {
        super();
      }

}
