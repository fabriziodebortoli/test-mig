import { chart } from './../../../../models/chart.model';
import { Component, ChangeDetectorRef, AfterViewInit, Input } from '@angular/core';
import { ChartType } from "./../../../../models/chart-type.model";
@Component({
    selector: 'rs-chart-bubble',
    templateUrl: './chart-bubble.component.html',
    styles: []
})

/**
 * This component includes chart types:
 *  bubble, scatter
 */

export class ReportChartBubbleComponent implements AfterViewInit {

    @Input() chart: chart
    constructor(private cdRef: ChangeDetectorRef) {

    }

    ngAfterViewInit() {
        this.cdRef.detectChanges();
    }
}
