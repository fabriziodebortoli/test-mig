import { ReportObjectType } from './report-object-type.model';
import { baserect } from './baserect.model';

export class sqrrect extends baserect {
    bkgcolor: string;
    constructor(jsonObj: any) {
        super(jsonObj.baserect);
        this.obj = ReportObjectType.sqrrect;
        this.bkgcolor = jsonObj.bkgcolor;
    };
}