import { chart, series } from './../../../../models/chart.model';
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
    constructor(public cdRef: ChangeDetectorRef) {

    }

    ngAfterViewInit() {
        this.cdRef.detectChanges();
    }

    getStack(item: series) {
        switch (this.chart.type) {
            case ChartType.AreaStacked:
            case ChartType.BarStacked:
            case ChartType.ColumnStacked: // "{ group: '' } " oppure valore del group
                return { group: item.group };
            case ChartType.AreaStacked100:
            case ChartType.BarStacked100:
            case ChartType.ColumnStacked100: // [stack]="{type:'100%', group: 'a' } || {type:'100%' }"
                return item.group ?
                    {
                        type: '100%',
                        group: item.group
                    } :
                    {
                        type: '100%'
                    };
            default:
                return false;
        }
    }

    getType(item: series): string {
        switch (item.type) {
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
    public labelContent(e: any): string {
        return e.value;
    }
}
