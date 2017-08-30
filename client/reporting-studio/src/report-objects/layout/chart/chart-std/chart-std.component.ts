import { ReportChartComponent } from './../chart.component';
import { Component, ChangeDetectorRef, AfterViewInit } from '@angular/core';
@Component({
    selector: 'rs-chart-std',
    templateUrl: './chart-std.component.html',
    styles: []
  })
  export class ReportChartStandardComponent extends ReportChartComponent implements AfterViewInit {

    constructor (cdRef: ChangeDetectorRef){
        super(cdRef);
    }

    ngAfterViewInit() {
        this.cdRef.detectChanges();
      }
    }
  