import { chart } from './../../../../models/chart.model';
import { Component, ChangeDetectorRef, AfterViewInit, Input } from '@angular/core';
import { ChartType } from "./../../../../models/chart-type.model";
@Component({
    selector: 'rs-chart-range-bar',
    templateUrl: './chart-range-bar.component.html',
    styles: []
})

/**
 * This component includes chart types:
 *  rangeColumn, rangeBar
 */

export class ReportChartRangeBarComponent implements AfterViewInit {

    @Input() chart: chart
    constructor(public cdRef: ChangeDetectorRef) {

    }

    ngAfterViewInit() {
        this.cdRef.detectChanges();
    }
}
