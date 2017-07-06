export class borderpen {
    width: number;
    color: string;
    constructor(jsonObj: any) {
        this.width = jsonObj.width;
        this.color = jsonObj.color;
    }
}