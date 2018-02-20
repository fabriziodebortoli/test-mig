import { barcode } from './barcode.model';
import { ReportObjectType } from './report-object-type.model';
import { baserect } from './baserect.model';
import { font } from './font.model';

export class textrect extends baserect {
    value: string;
    bkgcolor: string;
    textcolor: string;
    text_align: string;
    vertical_align: string;
    rotateBy: string;
    font: font;
    value_is_html: boolean;
    value_is_barcode: boolean;
    barcode: barcode;
    constructor(jsonObj: any) {
        super(jsonObj.baserect);
        this.obj = ReportObjectType.textrect;
        this.text_align = jsonObj.text_align;
        this.vertical_align = jsonObj.vertical_align;
        this.rotateBy = jsonObj.rotateBy;
        this.value = jsonObj.value ? jsonObj.value : '';
        this.bkgcolor = jsonObj.bkgcolor;
        this.textcolor = jsonObj.textcolor;
        this.font = new font(jsonObj.font);
        this.value_is_html = jsonObj.value_is_html;
        this.value_is_barcode = jsonObj.value_is_barcode;
        this.barcode = jsonObj.barcode ? new barcode(jsonObj.barcode) : undefined;
    };
}