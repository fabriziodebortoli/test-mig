export class ContextMenuItem {

    text: string;
    id: string;
    enabled: boolean = true;
    checked: boolean = false;
    dataBinding?: any; // TODO
    subItems: ContextMenuItem[];
    showMySub: boolean;
    fnc: any;

    public constructor(text: string, id: string, enabled: boolean = true, checked: boolean = false, subItems: ContextMenuItem[] = null, fnc: any = null) {
        this.text = text;
        this.id = id;
        this.enabled = enabled;
        this.checked = checked;
        this.subItems = subItems;
        this.showMySub = false;
        this.fnc = fnc;
    }
}
