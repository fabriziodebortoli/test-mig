import { dropdownListPair } from './dropdown-list-pair.model';
import { askObj } from './ask-obj.model';
import { AskObjectType } from './ask-object-type.model';

export class dropdownlist extends askObj {
    list: dropdownListPair[] = [];
    //set always visible because TB core inner control uses such property to show/hide itself
    //AskDialog manages visibility on the RS-Ask outer control 
    visible = true;
    constructor(jsonObj: any) {
        super(jsonObj);
        this.obj = AskObjectType.dropdownlist;
        for (let i = 0; i < jsonObj.list.length; i++) {
            let item = jsonObj.list[i];
            this.list.push(new dropdownListPair(item));
        }
    }

}