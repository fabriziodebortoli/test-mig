import { Component, Input, OnInit } from '@angular/core';
import { HttpService } from './../../../core/http.service';
import { EventDataService } from './../../../core/eventdata.service';
import { ControlComponent } from './../control.component';

@Component({
  selector: 'tb-linear-gauge',
  templateUrl: './linear-gauge.component.html',
  styleUrls: ['./linear-gauge.component.scss']
})
export class LinearGaugeComponent extends ControlComponent implements OnInit {

  @Input() maxRange:number;

  public bandColor:string;
  public bandOpacity:number;
  public bulletData: any = [5];
  public bulletValueAxis: any;

  ngOnInit() {
    this.bulletValueAxis = {
      min: 0,
      max: this.maxRange,
      plotBands: [{
          from: 0, to: this.maxRange, color: this.bandColor, opacity: this.bandOpacity
      }]
    };
  }

  constructor(private eventData: EventDataService) {
    super();
    this.bandColor = "#f0f0f0";
    this.bandOpacity = 1;
  }

  onBlur() {
    this.eventData.change.emit(this.cmpId);
  }  
}
