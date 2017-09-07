import { chart } from './../../../../models/chart.model';
import { Component, ChangeDetectorRef, AfterViewInit, Input } from '@angular/core';
import { ChartType } from "./../../../../models/chart-type.model";
@Component({
    selector: 'rs-chart-bar',
    templateUrl: './chart-bar.component.html',
    styles: []
})

/**
 * This component includes chart types:
 *  Bar, BarStacked, BarStacked100, Column, ColumnStacked, ColumnStacked100, Line
 */

export class ReportChartBarComponent implements AfterViewInit {

    @Input() chart: chart
    constructor(private cdRef: ChangeDetectorRef) {

    }

    ngAfterViewInit() {
        this.cdRef.detectChanges();
    }

    getStack() {
        switch (this.chart.type) {
            case ChartType.AreaStacked:
            case ChartType.BarStacked:
            case ChartType.ColumnStacked:
                return true;
            case ChartType.AreaStacked100:
            case ChartType.BarStacked100:
            case ChartType.ColumnStacked100:
                return {
                    type: '100%'
                };
            default:
                return false;
        }
    }

    getType(): string {
        switch (this.chart.type) {
            case ChartType.Bar:
            case ChartType.BarStacked:
            case ChartType.BarStacked100:
                return 'bar';
            case ChartType.Column:
            case ChartType.ColumnStacked:
            case ChartType.ColumnStacked100:
                return 'column';
            case ChartType.Area:
            case ChartType.AreaStacked:
            case ChartType.AreaStacked100:
                return 'area';
            case ChartType.Line:
                return 'line';
        }
    }
}
