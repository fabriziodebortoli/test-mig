export class ContextMenuItem {

    text: string;
    id: string;
    enabled: boolean;
    checked: boolean;
    dataBinding?: any; // TODO
    subItems: ContextMenuItem[];
    showMySub: boolean;

    public constructor(text: string, id: string, enabled: boolean, checked: boolean, subItems: ContextMenuItem[] = null) {
        this.text = text;
        this.id = id;
        this.enabled = enabled;
        this.checked = checked;
        this.subItems = subItems;
        this.showMySub = false;
    }
}