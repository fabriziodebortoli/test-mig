export class borders {
    left: boolean;
    right: boolean;
    top: boolean;
    bottom: boolean;
    constructor(jsonObj: any) {
        this.left = jsonObj.left;
        this.right = jsonObj.right;
        this.top = jsonObj.top;
        this.bottom = jsonObj.bottom;
    }
}