export declare class ContextMenuItem {
    text: string;
    id: string;
    enabled: boolean;
    checked: boolean;
    dataBinding?: any;
    subItems: ContextMenuItem[];
    showMySub: boolean;
    constructor(text: string, id: string, enabled: boolean, checked: boolean, subItems?: ContextMenuItem[]);
}
