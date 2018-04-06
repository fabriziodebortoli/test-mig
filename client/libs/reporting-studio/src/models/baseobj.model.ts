import { ReportObjectType } from './report-object-type.model';
import { rect } from './rect.model';

export class baseobj {
    obj: ReportObjectType;
    id: string;
    hidden: boolean;
    transparent: boolean;
    rect: rect;
    tooltip: string;
    shadow_height: number;
    shadow_color: string;
    constructor(jsonObj: any) {
        this.id = jsonObj.id;
        this.hidden = jsonObj.hidden;
        this.rect = new rect(jsonObj.rect);
        this.transparent = jsonObj.transparent;
        this.tooltip = jsonObj.tooltip ? jsonObj.tooltip : '';
        this.shadow_height = jsonObj.shadow_height ? jsonObj.shadow_height : 0;
        this.shadow_color  = jsonObj.shadow_color ? jsonObj.shadow_color : '#F0F8FF';
    };
}