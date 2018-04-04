export class dropdownListPair {
    code: string;
    description: string;
    constructor(jsonObj: any) {
        this.code = jsonObj.value;
        this.description = jsonObj.caption;
    }
}