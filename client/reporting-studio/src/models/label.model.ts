import { font } from './font.model';

export class label {
    caption: string;
    textcolor: string;
    font: font;
    text_align: string;
    vertical_align: string;
    rotateBy: string;
    constructor(jsonObj: any) {
        this.caption = jsonObj.caption ? jsonObj.caption : '';
        this.textcolor = jsonObj.textcolor;
        this.font = new font(jsonObj.font);
        this.text_align = jsonObj.text_align;
        this.vertical_align = jsonObj.vertical_align;
        this.rotateBy = jsonObj.rotateBy ? jsonObj.rotateBy : 0;
    }
}