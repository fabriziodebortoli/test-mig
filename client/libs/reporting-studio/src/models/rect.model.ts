export class rect {
    left: number;
    right: number;
    top: number;
    bottom: number;
    constructor(jsonObj: any) {
        this.left = jsonObj.left;
        this.right = jsonObj.right;
        this.top = jsonObj.top;
        this.bottom = jsonObj.bottom;
    };
}