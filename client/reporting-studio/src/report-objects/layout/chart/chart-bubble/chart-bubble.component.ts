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
 *  bubble, scatter, scatterLine
 */

export class ReportChartBubbleComponent implements AfterViewInit {

    @Input() chart: chart
    constructor(public cdRef: ChangeDetectorRef) {

    }

    ngAfterViewInit() {
        this.cdRef.detectChanges();
    }


    getType(item: any): string {
        switch (item.type) {
            case ChartType.Bubble:
            case ChartType.BubbleScatter:
                return 'bubble';
            case ChartType.Scatter:
                return 'scatter';
            case ChartType.ScatterLine:
                return 'scatterLine';
        }
    }
}
