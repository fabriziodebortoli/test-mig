import { ReportObjectType } from './report-object-type.model';
import { baserect } from './baserect.model';
import { font } from './font.model';

export class textrect extends baserect {
    value: string;
    bkgcolor: string;
    textcolor: string;
    text_align: string;
    vertical_align: string;
    font: font;
    value_is_html: boolean;
    value_is_barcode: boolean;
    constructor(jsonObj: any) {
        super(jsonObj.baserect);
        this.obj = ReportObjectType.textrect;
        this.text_align = jsonObj.text_align;
        this.vertical_align = jsonObj.vertical_align;
        this.value = jsonObj.value ? jsonObj.value : '';
        this.bkgcolor = jsonObj.bkgcolor;
        this.textcolor = jsonObj.textcolor;
        this.font = new font(jsonObj.font);
        this.value_is_html = jsonObj.value_is_html;
        this.value_is_barcode = jsonObj.value_is_barcode;
    };
}