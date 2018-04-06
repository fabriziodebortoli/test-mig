import { ReportObjectType } from './report-object-type.model';
import { sqrrect } from './sqrrect.model';

export class repeater extends sqrrect {
    rows: number;
    columns: number;
    xoffset: number;
    yoffset: number;
    constructor(jsonObj: any) {
        super(jsonObj.sqrrect);
        this.obj = ReportObjectType.repeater;
        this.rows = jsonObj.rows;
        this.columns = jsonObj.columns;
        this.xoffset = jsonObj.xoffset;
        this.yoffset = jsonObj.yoffset;
    };
}