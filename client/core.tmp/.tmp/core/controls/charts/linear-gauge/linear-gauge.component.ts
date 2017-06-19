import { Component, Input, OnInit } from '@angular/core';

import { ControlComponent } from './../../control.component';

import { EventDataService } from './../../../services/eventdata.service';

const DEFAULT_MAX_RANGE = 10;

@Component({
  selector: 'tb-linear-gauge',
  template: "<kendo-sparkline [data]=\"model?.nCurrentElement\" type=\"bullet\" [valueAxis]=\"rulerAxis\" (blur)=\"onBlur($event)\"></kendo-sparkline>",
  styles: [""]
})
export class LinearGaugeComponent extends ControlComponent implements OnInit {

  @Input() maxRange: number;

  public bandColor: string;
  public bandOpacity: number;
  public rulerAxis: any;

  ngOnInit() {

    if (this.maxRange == undefined) {
      this.maxRange = DEFAULT_MAX_RANGE;
    }

    this.rulerAxis = {
      min: 0,
      max: this.maxRange,
      plotBands: [{
        from: 0, to: this.maxRange, color: this.bandColor, opacity: this.bandOpacity
      }]
    };
  }

  constructor(private eventData: EventDataService) {
    super();
    this.setDefault();
  }

  setDefault() {
    this.bandColor = "#f0f0f0";
    this.bandOpacity = 1;
  }

  onBlur() {
    this.eventData.change.emit(this.cmpId);
  }
}
