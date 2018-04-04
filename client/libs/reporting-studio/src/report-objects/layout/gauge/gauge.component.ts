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

  applyStyle(): any {

    let obj = {
      'position': 'absolute',
      'top': this.gauge.rect.top + 'px',
      'left': this.gauge.rect.left + 'px',
     /* 'height': this.gauge.rect.bottom - this.gauge.rect.top + 'px',
      'width': this.gauge.rect.right - this.gauge.rect.left + 'px',
      'box-shadow': this.gauge.shadow_height + 'px ' + this.gauge.shadow_height + 'px '
      + this.gauge.shadow_height + 'px ' + this.gauge.shadow_color */

    };
    return obj;
  }
}
