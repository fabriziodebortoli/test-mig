import { baseobj } from './baseobj.model';
import { ReportObjectType } from "./report-object-type.model";
import { ChartType } from "./chart-type.model";


export class chart extends baseobj {

    type: ChartType;
    numSeries: number;
    title: string;
    legend: legend;
    category_title: string = "";
    categories: any[] = [];
    series: series[] = [];
    value: any;
    constructor(jsonObj: any) {
        super(jsonObj.baseobj);
        this.obj = ReportObjectType.chart;
        this.legend = jsonObj.legend ? new legend(jsonObj.legend) : undefined;
        this.type = jsonObj.chartType;
        this.numSeries = jsonObj.numSeries;
        this.title = jsonObj.title;
    }
}

export class legend {
    orientation: string;
    position: string;

    constructor(jsonObj) {
        this.orientation = jsonObj.oreintation ? jsonObj.oreintation : 'horizontal';
        this.position = jsonObj.position ? jsonObj.position : 'bottom';
    }
}

export class series {
    name: string;
    data: any[] = [];
    constructor(jsonObj: any) {
        this.name = jsonObj.name;
        this.data = jsonObj.data;
    }
}