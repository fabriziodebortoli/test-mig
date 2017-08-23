import { link } from './link.model';
import { font } from './font.model';
import { borderpen } from './borderpen.model';
import { borders } from './borders.model';

export class cell {
    id: string;
    borders: borders;
    pen: borderpen;
    textcolor: string;
    bkgcolor: string;
    text_align: string;
    vertical_align: string;
    font: font;
    tooltip: string = '';
    value: string = '';
    src: string;
    link: link = undefined;
    constructor(jsonObj: any, id: string) {
        this.id = id;
        this.borders = new borders(jsonObj.borders);
        this.pen = new borderpen(jsonObj.pen);
        this.textcolor = jsonObj.textcolor;
        this.bkgcolor = jsonObj.bkgcolor;
        this.text_align = jsonObj.text_align;
        this.vertical_align = jsonObj.vertical_align;
        this.font = new font(jsonObj.font);
        this.tooltip = jsonObj.tooltip ? jsonObj.tooltip : '';
        this.value = jsonObj.value ? jsonObj.value : '';
        this.link = jsonObj.link ? new link(jsonObj.link) : undefined;
    }
}