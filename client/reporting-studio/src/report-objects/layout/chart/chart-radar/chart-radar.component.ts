import { chart, series } from './../../../../models/chart.model';
import { Component, ChangeDetectorRef, AfterViewInit, Input } from '@angular/core';
import { ChartType } from "./../../../../models/chart-type.model";
@Component({
    selector: 'rs-chart-radar',
    templateUrl: './chart-radar.component.html',
    styles: []
})

/**
 * This component includes chart types:
 *   radar
 */

export class ReportChartRadarComponent implements AfterViewInit {

    @Input() chart: chart

    constructor(public cdRef: ChangeDetectorRef) {
    }

    ngAfterViewInit() {
        this.cdRef.detectChanges();
    }

    getType(item: any): string {
        switch (item.type) {
            case ChartType.RadarArea:
                return 'radarArea';
            case ChartType.RadarLine:
                return 'radarLine';
        }
    }
}
