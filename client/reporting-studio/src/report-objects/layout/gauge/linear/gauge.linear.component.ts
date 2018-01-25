import { gauge } from './../../../../models/gauge.model';
import { Component, Input, Type } from '@angular/core';


@Component({
  selector: 'rs-gauge-linear',
  templateUrl: './gauge.linear.component.html',
  styles: []
})
export class ReportGaugeLinearComponent {

  @Input() gauge: gauge;
  constructor() {
  }


}
