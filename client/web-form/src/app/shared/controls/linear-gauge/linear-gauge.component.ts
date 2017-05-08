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

  @Input () markerData;

  public data: any[] = [this.markerData];
  public bulletValueAxis: any = {
    min: 0,
    max: 30,
    plotBands: [{
        from: 0, to: 30, color: "#f0f0f0", opacity: 1
    }]
  };

  constructor(private eventData: EventDataService) {
    super();
  }

  onBlur() {
    this.eventData.change.emit(this.cmpId);
  }  
}
