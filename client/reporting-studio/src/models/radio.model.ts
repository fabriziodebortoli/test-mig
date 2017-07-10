import { AskObjectType } from './ask-object-type.model';
import { askObj } from './ask-obj.model';

export class radio extends askObj {
    group_name: string;
    constructor(jsonObj: any) {
        super(jsonObj);
        this.group_name = jsonObj.group_name;
        this.obj = AskObjectType.radio;
    }
}