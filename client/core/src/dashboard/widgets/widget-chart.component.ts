import { WidgetComponent } from './widget.component';

import { Component, OnInit, Input, AfterViewInit, ViewChild, AfterContentInit } from '@angular/core';
import { CategoryAxis, SeriesLabels } from '@progress/kendo-angular-charts/dist/es/common/property-types';
import { Widget } from "./widgets.service";

@Component({
  selector: 'tb-widget-chart',
  templateUrl: './widget-chart.component.html',
  styleUrls: ['./widget-chart.component.scss']
})
export class WidgetChartComponent implements AfterContentInit {
  @Input() widget: Widget;

  categoryAxis: CategoryAxis = null;
  visibleTooltip = false;
  seriesDefaults = null; // cannot use SeriesDefaults type!!!
  seriesLabels: SeriesLabels = null;

  @ViewChild('chart') chart;

  constructor(public widgetComponent: WidgetComponent) { }

  ngAfterContentInit() {

    // for column type, shorten the label if too long
    if (this.widget.layout.chartFormat.type === 'column') {
      this.categoryAxis = { labels: { content: this.labelContent } };
      this.visibleTooltip = true; // provide a tooltip to show the full description
    }
    else if (this.widget.layout.chartFormat.type === 'pie' || this.widget.layout.chartFormat.type === 'donut') {
      this.seriesDefaults = {
        labels: {
          position: 'outside',
          distance: 5,
          visible: true,
          background: 'transparent'
        }
      };
      this.seriesLabels = {
        visible: true,
        align: 'column',
        content: this.seriesLabelContent,
        font: '11px Roboto,sans-serif'
      };
    }

    // pie and donuts, enlarge to fill more space on the bottom
    if (this.widget.layout.chartFormat.type === 'pie' || this.widget.layout.chartFormat.type === 'donut') {
      this.chart.element.nativeElement.style.height = '100%';
    }
  }

  chartColor(colorName: string) {
    if (colorName == undefined)
      return '#03A2FF';

    switch (colorName) {
      case 'green': return '#66bb6a';
      case 'purple': return '#ab47bc';
      case 'blue': return '#03A2FF';
      case 'orange': return '#ffa726';
      case 'red': return '#ef5350';
      case 'cyan': return '#26c6da';
      default: return '#03A2FF';
    }
  }

  labelContent(e: any) {
    if (e.value.length > 10) {
      return e.value.slice(0, 10) + '...';
    } else {
      return e.value;
    }
  }

  seriesLabelContent(e: any) {
    return (e.percentage * 100).toFixed(2) + ' %';
  }

}
