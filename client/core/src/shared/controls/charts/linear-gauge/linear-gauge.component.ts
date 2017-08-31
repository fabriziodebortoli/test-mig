import { TbComponentService } from './../../../../core/services/tbcomponent.service';
import { LayoutService } from './../../../../core/services/layout.service';
import { Component, Input, OnInit } from '@angular/core';

import { EventDataService } from './../../../../core/services/eventdata.service';

import { ControlComponent } from './../../control.component';

const DEFAULT_MAX_RANGE: number = 10;

@Component({
  selector: 'tb-linear-gauge',
  templateUrl: './linear-gauge.component.html',
  styleUrls: ['./linear-gauge.component.scss']
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

  constructor(private eventData: EventDataService, layoutService: LayoutService, tbComponentService:TbComponentService) {
    super(layoutService, tbComponentService);
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
