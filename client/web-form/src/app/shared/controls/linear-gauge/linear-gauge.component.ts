import { Component, Input } from '@angular/core';
import { HttpService } from './../../../core/http.service';
import { EventDataService } from './../../../core/eventdata.service';
import { ControlComponent } from './../control.component';

@Component({
  selector: 'tb-linear-gauge',
  templateUrl: './linear-gauge.component.html',
  styleUrls: ['./linear-gauge.component.scss']
})
export class LinearGaugeComponent extends ControlComponent {

  constructor(private eventData: EventDataService) {
    super();
  }

  onBlur() {
    this.eventData.change.emit(this.cmpId);
  }  
}
