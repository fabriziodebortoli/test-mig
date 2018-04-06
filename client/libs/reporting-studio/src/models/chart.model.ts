import { baserect } from './baserect.model';

import { ReportObjectType } from "./report-object-type.model";
import { ChartType } from "./chart-type.model";


export class chart extends baserect {

    type: ChartType;
    title: string;
    legend: legend;
    category_title: string = "";
    categories: any[] = [];
    series: series[] = [];
    value: any;
    constructor(jsonObj: any) {
        super(jsonObj.baserect);
        this.obj = ReportObjectType.chart;
        this.legend = jsonObj.legend ? new legend(jsonObj.legend) : undefined;
        this.type = jsonObj.chartType;
        this.title = jsonObj.title;
    }
}

export class legend {
    orientation: string='';
    position: string;

    constructor(jsonObj) {
       // this.orientation = jsonObj.oreintation ? jsonObj.oreintation : 'horizontal';
        this.position = jsonObj.position ? jsonObj.position : 'bottom';
    }
}

export class series {

    name: string;
    type: ChartType;
    color: string;
    group: any;
    style: string;
    data: any[] = [];
    label: boolean;
    opacity: number;
    constructor(jsonObj: any) {
        this.name = jsonObj.name;
        this.data = jsonObj.data;
        this.type = jsonObj.type;
        this.style = jsonObj.style;
        this.color = jsonObj.color ? jsonObj.color : '';
        this.group = jsonObj.group ? jsonObj.group : '';
        this.opacity = jsonObj.transparent ? jsonObj.transparent : 1;
    }
}