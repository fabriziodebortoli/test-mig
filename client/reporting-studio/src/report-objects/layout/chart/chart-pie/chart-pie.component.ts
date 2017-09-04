import { chart } from './../../../../models/chart.model';
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
  export class ReportChartPieComponent implements AfterViewInit {

    @Input() chart:chart

    constructor (private cdRef: ChangeDetectorRef){

    }

    ngAfterViewInit() {
        this.cdRef.detectChanges();
      }
  }
  