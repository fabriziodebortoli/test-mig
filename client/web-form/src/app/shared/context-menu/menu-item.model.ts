export class MenuItem {
    text: string;
    id: string;
    enabled: boolean;
    checked: boolean;
    dataBinding?: any; // TODO 
    subItems: MenuItem[];

    public constructor(text: string, id: string, enabled: boolean, checked: boolean, subItems: MenuItem[] = null) {
        this.text = text;
        this.id = id;
        this.enabled = enabled;
        this.checked = checked;
        this.subItems = subItems;
    }
}
