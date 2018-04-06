import { AskObjectType } from './ask-object-type.model';
import { fieldAskObj } from './field-ask-obj.model';

export class askObj extends fieldAskObj {
    obj: AskObjectType;
    hidden: boolean;
    enabled: boolean;
    caption: string;
    left_aligned: boolean;
    left_text: boolean;
    runatserver: boolean;

    constructor(jsonObj: any) {
        super(jsonObj.field);
        this.hidden = jsonObj.hidden;
        this.enabled = jsonObj.enabled;
        this.caption = jsonObj.caption;
        this.left_aligned = jsonObj.left_aligned;
        this.left_text = jsonObj.left_text;
        this.runatserver = jsonObj.runatserver;

    }
}