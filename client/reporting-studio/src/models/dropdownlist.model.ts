import { dropdownListPair } from './dropdown-list-pair.model';
import { askObj } from './ask-obj.model';
import { AskObjectType } from './ask-object-type.model';

export class dropdownlist extends askObj {
    list: dropdownListPair[] = [];
    constructor(jsonObj: any) {
        super(jsonObj);
        this.obj = AskObjectType.dropdownlist;
        for (let i = 0; i < jsonObj.list.length; i++) {
            let item = jsonObj.list[i];
            this.list.push(new dropdownListPair(item));
        }
    }

}