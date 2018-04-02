export class font {
    face: string;
    size: number;
    italic: boolean;
    bold: boolean;
    underline: boolean;
    fontcolor: string;

    constructor(jsonObj: any) {
        this.face = jsonObj.face;
        this.size = jsonObj.size;
        this.italic = jsonObj.italic ? jsonObj.italic : false;
        this.bold = jsonObj.bold ? jsonObj.bold : false;
        this.underline = jsonObj.underline ? jsonObj.underline : false;
        this.fontcolor =  jsonObj.fontcolor ? jsonObj.fontcolor : "#000000";
    }
}