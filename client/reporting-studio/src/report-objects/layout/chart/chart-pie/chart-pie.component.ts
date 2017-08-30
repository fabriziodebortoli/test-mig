import { ReportChartComponent } from './../chart.component';
import { Component, ChangeDetectorRef, AfterViewInit } from '@angular/core';
@Component({
    selector: 'rs-chart-pie',
    templateUrl: './chart-pie.component.html',
    styles: []
  })
  export class ReportChartPieComponent extends ReportChartComponent implements AfterViewInit {

    constructor (cdRef: ChangeDetectorRef){
        super(cdRef);
    }

    ngAfterViewInit() {
        this.cdRef.detectChanges();
      }
  }
  