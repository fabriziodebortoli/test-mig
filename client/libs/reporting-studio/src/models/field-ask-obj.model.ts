export class fieldAskObj {
    name: string;
    id: string;
    type: string;
    value: any;
    constructor(jsonObj: any) {
        this.name = jsonObj.name;
        this.id = jsonObj.id;
        this.type = jsonObj.type;
        this.value = jsonObj.value;
    }
}