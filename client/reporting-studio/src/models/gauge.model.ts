import { ComboSimpleComponent } from '@taskbuilder/core';
import { baserect } from './baserect.model';


export enum GaugeObjectType { arc, linear, radial }

export class gauge extends baserect {
    value: number = 0;
    type: GaugeObjectType;
    scale: scale;
    ranges: range[];
    pointers: point[];
    constructor(jsonObj: any) {
        super(jsonObj.baserect);
        this.value = jsonObj.value;
        this.type = jsonObj.Type;
        this.scale = new scale(jsonObj.scale);
        if (jsonObj.ranges) {
            jsonObj.ranges.forEach(element => {
                this.ranges.push(new range(element));
            });
        }
        if (jsonObj.pointers) {
            jsonObj.pointers.forEach(element => {
                this.pointers.push(new point(element));
            });
        }
    }
}

export class scale {
    max: number;
    min: number;
    // rangePlaceholderColor:string;
    constructor(jsonObj: any) {
        this.max = jsonObj.max;
        this.min = jsonObj.min;
        // this.rangePlaceholderColor=
    }
}

export class range {
    from: number;
    to: number;
    color: string;

    constructor(jsonObj: any) {
        this.from = jsonObj.from;
        this.to = jsonObj.to;
        this.color = jsonObj.color;
    }
}

export class point {
    value: number;
    color: string;

    constructor(jsonObj: any) {
        this.value = jsonObj.value;
        this.color = jsonObj.color;
    }
}
