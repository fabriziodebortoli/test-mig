import { gauge } from './../../../../models/gauge.model';
import { Component, Input, } from '@angular/core';


@Component({
  selector: 'rs-gauge-radial',
  templateUrl: './gauge.radial.component.html',
  styles: []
})
export class ReportGaugeRadialComponent {

  @Input() gauge: gauge;
  constructor() {
  }


}
