import { Component, Input, OnInit, SimpleChanges,ChangeDetectorRef } from '@angular/core';

import { TbComponentService } from './../../../../core/services/tbcomponent.service';
import { LayoutService } from './../../../../core/services/layout.service';
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

  constructor(
    public eventData: EventDataService, 
    layoutService: LayoutService, 
    tbComponentService: TbComponentService,
    changeDetectorRef:ChangeDetectorRef) {
    super(layoutService, tbComponentService, changeDetectorRef);
    this.setDefault();
  }

  setDefault() {
    this.bandColor = "#f0f0f0";
    this.bandOpacity = 1;
  }

  onBlur(changes: SimpleChanges) {
    this.eventData.change.emit(this.cmpId);
  }
}
