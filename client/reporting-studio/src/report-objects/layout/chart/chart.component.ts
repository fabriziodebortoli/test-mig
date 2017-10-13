import { chart } from './../../../models/chart.model';
import { SeriesType } from '@progress/kendo-angular-charts';
import { Component, Input, ChangeDetectorRef, AfterViewInit } from '@angular/core';
import { ChartType} from "./../../../models/chart-type.model";
import { NgSwitch } from '@angular/common';

@Component({
  selector: 'rs-chart',
  templateUrl: './chart.component.html',
  styles: []
})


export class ReportChartComponent implements AfterViewInit {

  @Input() chart: chart;

  public CT = ChartType;
  constructor(public cdRef: ChangeDetectorRef) { }

  ngAfterViewInit() {
    this.cdRef.detectChanges();
  }

  applyStyle(): any {

    let obj = {
      'position': 'absolute',
      'top': this.chart.rect.top + 'px',
      'left': this.chart.rect.left + 'px',
      'height': this.chart.rect.bottom - this.chart.rect.top + 'px',
      'width': this.chart.rect.right - this.chart.rect.left + 'px',
      'box-shadow': this.chart.shadow_height + 'px ' + this.chart.shadow_height + 'px '
      + this.chart.shadow_height + 'px ' + this.chart.shadow_color

    };
    return obj;
  }
}