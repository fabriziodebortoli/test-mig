import { borderpen } from './borderpen.model';
import { borders } from './borders.model';
import { font } from './font.model';
import { rect } from './rect.model';

export class title {
    rect: rect;
    caption: string;
    pen: borderpen;
    borders: borders;
    textcolor: string;
    bkgcolor: string;
    text_align: string;
    vertical_align: string;
    rotateBy: string;
    font: font;
    tooltip: string = '';
    constructor(jsonObj: any) {
        this.rect = new rect(jsonObj.rect);
        this.caption = jsonObj.caption;
        this.pen = new borderpen(jsonObj.pen);
        this.borders = new borders(jsonObj.borders);
        this.textcolor = jsonObj.textcolor;
        this.bkgcolor = jsonObj.bkgcolor;
        this.text_align = jsonObj.text_align;
        this.vertical_align = jsonObj.vertical_align;
        this.rotateBy = jsonObj.rotateBy ? jsonObj.rotateBy : 0;
        this.font = new font(jsonObj.font);
        this.tooltip = jsonObj.tooltip ? jsonObj.tooltip : '';
    }
}