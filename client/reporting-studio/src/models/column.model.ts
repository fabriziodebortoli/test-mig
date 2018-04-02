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
        this.hidden = jsonObj.hidden ? jsonObj.hidden : false;
        this.width = jsonObj.width;
        this.value_is_html = jsonObj.value_is_html ? jsonObj.value_is_html : false;
        this.value_is_image = jsonObj.value_is_image ? jsonObj.value_is_image : false;
        this.value_is_barcode = jsonObj.value_is_barcode ? jsonObj.value_is_barcode : false;
        this.title = jsonObj.title ? new title(jsonObj.title) : undefined;
        this.fit_mode = jsonObj.fit_mode ? jsonObj.fit_mode : ImgFitMode.BEST;
        // this.total = jsonObj.total ? new column_total(jsonObj.total) : undefined;
    }
}