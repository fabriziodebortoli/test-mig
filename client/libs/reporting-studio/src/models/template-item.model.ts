export class TemplateItem {
    public templateName: string;
    public templateObjects: any[] = [];
    public template: any;

    constructor(tName: string, template: any, tObj: any[]) {
        this.templateName = tName;
        this.templateObjects = tObj;
        this.template = template;
    }
}