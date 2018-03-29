import { title } from './title.model';
import { ImgFitMode } from './image-fit-mode.model';

export class column {
    id: string;
    hidden: boolean;
    width: number;
    value_is_html: boolean;
    value_is_image: boolean;
    value_is_barcode: boolean;
    title: title;
    fit_mode: ImgFitMode;
    //total: column_total;

    constructor(jsonObj: any) {
        this.id = jsonObj.id;
        this.hidden = jsonObj.hidden;
        this.width = jsonObj.width;
        this.value_is_html = jsonObj.value_is_html;
        this.value_is_image = jsonObj.value_is_image;
        this.value_is_barcode = jsonObj.value_is_barcode;
        this.title = jsonObj.title ? new title(jsonObj.title) : undefined;
        // this.total = jsonObj.total ? new column_total(jsonObj.total) : undefined;
    }
}