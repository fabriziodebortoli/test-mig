import { AskObjectType } from './ask-object-type.model';
import { askObj } from './ask-obj.model';

export class hotlink extends askObj {
    ns: string;
    selectionList: string[] = [];
    selection_type: string;
    multi_selection: boolean;
    args: any;
    constructor(jsonObj: any) {
        super(jsonObj);
        this.obj = AskObjectType.hotlink;
        this.ns = jsonObj.hotlink.ns;
        this.multi_selection = jsonObj.hotlink.multi_selection;
        this.selection_type = 'code';
        this.args = jsonObj.hotlink.args;
        this.value = jsonObj.field.value;
    }

}