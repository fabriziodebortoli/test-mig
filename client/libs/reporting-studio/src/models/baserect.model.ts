import { baseobj } from './baseobj.model';
import { borders } from './borders.model';
import { borderpen } from './borderpen.model';

export class baserect extends baseobj {
    borders: borders;
    ratio: number;
    pen: borderpen;
    constructor(jsonObj: any) {
        super(jsonObj.baseobj);
        this.ratio = jsonObj.ratio;
        this.borders = new borders(jsonObj.borders);
        this.pen = new borderpen(jsonObj.pen);
    };
}