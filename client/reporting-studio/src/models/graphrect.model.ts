import { ImgFitMode } from './image-fit-mode.model';
import { ReportObjectType } from './report-object-type.model';
import { sqrrect } from './sqrrect.model';


export class graphrect extends sqrrect {
    value: string;
    text_align: string;
    vertical_align: string;
    fit_mode: ImgFitMode;
    src: string = '';

    constructor(jsonObj: any) {
        super(jsonObj.sqrrect !== undefined ? jsonObj.sqrrect : jsonObj); // if image is constructed from fieldRect the jsonObj, else jsonObj.sqrrect
        this.obj = ReportObjectType.graphrect;
        this.text_align = jsonObj.text_align;
        this.vertical_align = jsonObj.vertical_align;
        this.value = jsonObj.image ? jsonObj.image : '';
        this.fit_mode = jsonObj.fit_mode;
    };
}
