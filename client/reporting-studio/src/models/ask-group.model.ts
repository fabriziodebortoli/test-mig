import { hotlink } from './hotlink.model';
import { dropdownlist } from './dropdownlist.model';
import { radio } from './radio.model';
import { check } from './check.model';
import { text } from './text.model';
import { askObj } from './ask-obj.model';

export class askGroup {
    caption: string;
    hidden: boolean;
    group_name: string;
    entries: askObj[] = [];
    radioBtns: radio[] = [];
    isRadioGroup: boolean = false;
    constructor(jsonObj: any) {
        this.caption = jsonObj.caption;
        this.hidden = jsonObj.hidden;
        this.group_name = jsonObj.group_name;
        for (let i = 0; i < jsonObj.entries.length; i++) {
            let element = jsonObj.entries[i];
            let obj;

            if (element.text !== undefined) {
                obj = new text(element.text);
            }
            else if (element.check !== undefined) {
                obj = new check(element.check);
            }
            else if (element.radio !== undefined) {
                this.isRadioGroup = true;
                obj = new radio(element.radio);
                this.radioBtns.push(obj);
            }
            else if (element.dropdownlist !== undefined) {
                obj = new dropdownlist(element.dropdownlist);
            }
            else if (element.textwithhotlink !== undefined) {
                obj = new hotlink(element.textwithhotlink);
            }

            this.entries.push(obj);
        }
    }
}