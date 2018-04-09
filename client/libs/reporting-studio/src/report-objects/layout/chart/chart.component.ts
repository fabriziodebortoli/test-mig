import { rect } from './../../../models/rect.model';
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
      'overflow':'hidden',
      'top': this.chart.rect.top + 'px',
      'left': this.chart.rect.left + 'px',
      'height': this.chart.rect.bottom - this.chart.rect.top + 'px',
      'width': this.chart.rect.right - this.chart.rect.left + 'px',
      'border-left': this.chart.borders.left ? this.chart.pen.width + 'px' : '0px',
      'border-right': this.chart.borders.right ? this.chart.pen.width + 'px' : '0px',
      'border-bottom': this.chart.borders.bottom ? this.chart.pen.width + 'px' : '0px',
      'border-top': this.chart.borders.top ? this.chart.pen.width + 'px' : '0px',
      'border-color': this.chart.pen.color,
      'border-radius': this.chart.ratio + 'px',
      'border-style': 'solid',
      'box-shadow': this.chart.shadow_height + 'px ' + this.chart.shadow_height + 'px '
      + this.chart.shadow_height + 'px ' + this.chart.shadow_color

    };
    return obj;
  }
}