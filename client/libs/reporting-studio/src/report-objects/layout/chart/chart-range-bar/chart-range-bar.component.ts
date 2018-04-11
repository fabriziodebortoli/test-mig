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


    getType(item: any): string {
        switch (item.type) {
            case ChartType.RangeArea:
                return 'rangeArea';
            case ChartType.RangeBar:
                return 'rangeBar';
                case ChartType.RadarColumn:
                return 'rangeColumn';
        }  
    }

    public labelContentFrom(e: any): string {
        return  e.value.from ;
        //return `${ e.value.from } `;
    }
  
    public labelContentTo(e: any): string {
        return  e.value.to ;
        //return `${ e.value.to } `;
    }
}
