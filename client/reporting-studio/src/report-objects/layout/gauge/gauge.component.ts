import { gauge, GaugeObjectType } from './../../../models/gauge.model';
import { Component, Input, } from '@angular/core';


@Component({
  selector: 'rs-gauge',
  templateUrl: './gauge.component.html',
  styles: []
})
export class ReportGaugeComponent {

  @Input() gauge: gauge;
  public GOT = GaugeObjectType;
  constructor() {
  }


}
