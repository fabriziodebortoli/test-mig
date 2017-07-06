export class font {
    face: string;
    size: number;
    italic: boolean;
    bold: boolean;
    underline: boolean;
    constructor(jsonObj: any) {
        this.face = jsonObj.face;
        this.size = jsonObj.size;
        this.italic = jsonObj.italic;
        this.bold = jsonObj.bold;
        this.underline = jsonObj.underline;
    }
}