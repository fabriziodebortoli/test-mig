import { AskObjectType } from './ask-object-type.model';
import { askObj } from './ask-obj.model';

export class text extends askObj {
    //viene settato il visible perchè il controllo condiviso nella core richiede la visibility
    visible = true;
    constructor(jsonObj: any) {
        super(jsonObj);
        this.obj = AskObjectType.text;
    }
}