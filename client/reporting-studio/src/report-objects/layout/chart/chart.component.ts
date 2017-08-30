import { chart } from './../../../models/chart.model';
import { SeriesType } from '@progress/kendo-angular-charts';
import { Component, Input, ChangeDetectorRef, AfterViewInit } from '@angular/core';
import { ChartType } from "./../../../models/chart-type.model";

@Component({
  selector: 'rs-chart',
  templateUrl: './chart.component.html',
  styles: []
})
export class ReportChartComponent implements AfterViewInit {

  @Input() chart: chart;

  constructor(public cdRef: ChangeDetectorRef) {
  }

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

  getChartType(): SeriesType {
    switch (this.chart.type) {
      case ChartType.Bar:
      case ChartType.BarStacked:
      case ChartType.BarStacked100:
        return 'bar';
      case ChartType.Column:
      case ChartType.ColumnStacked:
      case ChartType.ColumnStacked100:
        return 'column';
      case ChartType.Line:
        return 'line';
      case ChartType.VerticalLine:
        return 'verticalLine';
      case ChartType.Area:
      case ChartType.AreaStacked:
      case ChartType.AreaStacked100:
        return 'area';
      case ChartType.VerticalArea:
        return 'verticalArea';
     case ChartType.Pie:
        return 'pie';
     case ChartType.Donut:
        return 'donut';
    case ChartType.DonutNested:
        return 'donut';
    case ChartType.Funnel:
        return 'funnel';
  case ChartType.Pyramid:
        return 'funnel';

    }
  }

  getStack(): any {

    switch (this.chart.type) {
      case ChartType.Bar:
      case ChartType.Column:
      case ChartType.Line:
      case ChartType.Area:
      case ChartType.VerticalArea:
        return false;

      case ChartType.BarStacked:
      case ChartType.ColumnStacked:
      case ChartType.AreaStacked:
        return true;

      case ChartType.BarStacked100:
      case ChartType.ColumnStacked100:
      case ChartType.AreaStacked100:
        return {
          type: '100%'
       };
    }
  }
}