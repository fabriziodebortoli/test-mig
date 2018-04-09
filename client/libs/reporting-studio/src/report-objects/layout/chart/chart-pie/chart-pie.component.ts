import { chart, series } from './../../../../models/chart.model';
import { Component, ChangeDetectorRef, AfterViewInit, Input } from '@angular/core';
import { ChartType } from "./../../../../models/chart-type.model";
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

    @Input() chart: chart

    constructor(public cdRef: ChangeDetectorRef) {

    }

    ngAfterViewInit() {
        this.cdRef.detectChanges();
    }

    getType(item: series): string {
        switch (item.type) {
            case ChartType.Pie:
                return 'pie';
            case ChartType.Donut:
            case ChartType.DonutNested:
                return 'donut';
            case ChartType.Funnel:
                return 'funnel';
        }
    }

    public labelContent(e: any): string {
        return e.category + ", " + (Math.round(e.percentage * 1000)* 0.1 ).toPrecision(3) + '%';
    }
}
