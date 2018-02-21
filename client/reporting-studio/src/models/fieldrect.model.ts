import { barcode } from './barcode.model';
import { ReportObjectType } from './report-object-type.model';
import { baserect } from './baserect.model';
import { label } from './label.model';
import { font } from './font.model';
import { link } from './link.model';

export class fieldrect extends baserect {
    value: string = '';
    label: label;
    font: font;
    text_align: string;
    vertical_align: string;
    rotateBy: string;
    bkgcolor: string;
    textcolor: string;
    value_is_html: boolean;
    value_is_image: boolean;
    value_is_barcode: boolean;
    link: link = undefined;
    barcode: barcode;
    constructor(jsonObj: any) {
        super(jsonObj.baserect);
        this.obj = ReportObjectType.fieldrect;
        this.label = jsonObj.label ? new label(jsonObj.label) : undefined;
        this.font = new font(jsonObj.font);
        this.text_align = jsonObj.text_align;
        this.vertical_align = jsonObj.vertical_align;
        this.rotateBy = jsonObj.rotateBy ? jsonObj.rotateBy : 0;
        this.bkgcolor = jsonObj.bkgcolor;
        this.textcolor = jsonObj.textcolor;
        this.value_is_html = jsonObj.value_is_html;
        this.value_is_image = jsonObj.value_is_image;
        this.value_is_barcode = jsonObj.value_is_barcode;
        this.link = jsonObj.link ? new link(jsonObj.link) : undefined;
        this.barcode = jsonObj.barcode ? new barcode(jsonObj.barcode) : undefined;
    };
}