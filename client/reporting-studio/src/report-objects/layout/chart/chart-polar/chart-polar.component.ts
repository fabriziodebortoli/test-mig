import { chart } from './../../../../models/chart.model';
import { Component, ChangeDetectorRef, AfterViewInit, Input } from '@angular/core';
import { ChartType } from "./../../../../models/chart-type.model";

@Component({
    selector: 'rs-chart-polar',
    templateUrl: './chart-polar.component.html',
    styles: []
})

/**
 * This component includes chart types:
 *  polar
 */

export class ReportChartPolarComponent implements AfterViewInit {

    @Input() chart: chart
    constructor(private cdRef: ChangeDetectorRef) {

    }

    ngAfterViewInit() {
        this.cdRef.detectChanges();
    }
}
