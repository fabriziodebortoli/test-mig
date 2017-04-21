export class MenuItem {
    text: string;
    id: string;
    enabled: boolean;
    checked: boolean;
    dataBinding?: any; // TODO 
    subItems: MenuItem[];
}
