import { chart, series } from './../../../../models/chart.model';
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
        let k=this.chart;
    }

    getType(item: series): string {
        switch (item.type) {
            case ChartType.PolarArea:
                return 'polarArea';
            case ChartType.PolarLine:
                return 'polarLine';
            case ChartType.PolarScatter:
                return 'polarScatter';
        }  
    }

    public labelContent(e: any): string {
        return e.y;
    }
}
