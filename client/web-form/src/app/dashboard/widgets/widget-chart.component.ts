import { WidgetComponent } from './widget.component';
import { Widget } from './widgets.service';
import { Component, OnInit, Input, AfterViewInit, ViewChild } from '@angular/core';
import { CategoryAxis, SeriesLabels } from '@progress/kendo-angular-charts/dist/es/common/property-types';

@Component({
  selector: 'tb-widget-chart',
  templateUrl: './widget-chart.component.html',
  styles: [`
    .k-chart {
      height: 100%;
    }
  `]
})
export class WidgetChartComponent implements OnInit, AfterViewInit {
  @Input() widget: Widget;

  categoryAxis: CategoryAxis = null;
  visibleTooltip = false;
  seriesDefaults = null; // cannot use SeriesDefaults type!!!
  seriesLabels: SeriesLabels = null;

  @ViewChild('chart') chart;

  constructor(private widgetComponent: WidgetComponent) { }

  ngOnInit() {
    // for column type, shorten the label if too long
    if (this.widget.layout.chartFormat.type === 'column') {
      this.categoryAxis = { labels: { content: this.labelContent } };
      this.visibleTooltip = true; // provide a tooltip to show the full description
    } else if (this.widget.layout.chartFormat.type === 'pie' || this.widget.layout.chartFormat.type === 'donut') {
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
          font: '11px Open Sans,sans-serif'
        };
    }
  }

  ngAfterViewInit() {
    // pie and donuts, enlarge to fill more space on the bottom
    if (this.widget.layout.chartFormat.type === 'pie' || this.widget.layout.chartFormat.type === 'donut') {
      this.chart.element.nativeElement.style.height = '120%';
    }
  }

  chartColor(colorName: string) {
    switch (colorName) {
      case 'green' : return '#66bb6a';
      case 'purple' : return '#ab47bc';
      case 'blue' : return '#03A2FF';
      case 'orange' : return '#ffa726';
      case 'red' : return '#ef5350';
      case 'cyan' : return '#26c6da';
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
