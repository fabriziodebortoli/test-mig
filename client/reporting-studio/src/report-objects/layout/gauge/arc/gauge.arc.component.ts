import { gauge } from './../../../../models/gauge.model';

import { Component, Input, } from '@angular/core';


@Component({
  selector: 'rs-gauge-arc',
  templateUrl: './gauge.arc.component.html',
  styles: []
})
export class ReportGaugeArcComponent {

  @Input() gauge: gauge;

  constructor() {
  }


}
