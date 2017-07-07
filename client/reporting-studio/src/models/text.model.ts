import { AskObjectType } from './ask-object-type.model';
import { askObj } from './ask-obj.model';

export class text extends askObj {
    constructor(jsonObj: any) {
        super(jsonObj);
        this.obj = AskObjectType.text;
    }
}