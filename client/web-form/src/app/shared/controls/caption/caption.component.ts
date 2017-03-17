import { EventDataService } from './../../../core/eventdata.service';
import { ControlComponent } from './../control.component';
import { Component } from '@angular/core';

@Component({
  selector: 'tb-caption',
  templateUrl: './caption.component.html',
  styleUrls: ['./caption.component.scss']
})
export class CaptionComponent extends ControlComponent {

 constructor(
        private eventData: EventDataService
      ) {
        super();
      }
}
