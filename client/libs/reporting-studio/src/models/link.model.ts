import { ReportObjectType } from './report-object-type.model';
import { LinkType } from './link-type.model';

export class link {
    obj: ReportObjectType = ReportObjectType.link;
    type: LinkType;
    ns: string;
    arguments: string;
    runAtServer: boolean = false;
    constructor(jsonObj: any) {
        this.type = jsonObj.type;
        this.ns = jsonObj.ns;
        this.arguments = jsonObj.arguments;
        this.runAtServer = jsonObj.runAtServer ? jsonObj.runAtServer : false;
    }
}