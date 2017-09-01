import { chart } from './../../../../models/chart.model';
import { ReportChartComponent } from './../chart.component';
import { Component, ChangeDetectorRef, AfterViewInit, Input } from '@angular/core';
@Component({
    selector: 'rs-chart-pie',
    templateUrl: './chart-pie.component.html',
    styles: []
  })

  
/**
 * This component includes chart types:
 *  Pie, Donut, Funnel
 */
  export class ReportChartPieComponent extends ReportChartComponent implements AfterViewInit {

    @Input() chart:chart

    constructor (cdRef: ChangeDetectorRef){
        super(cdRef);
    }

    ngAfterViewInit() {
        this.cdRef.detectChanges();
      }
  }
  