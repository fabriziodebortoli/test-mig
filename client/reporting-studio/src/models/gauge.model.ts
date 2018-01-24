import { ComboSimpleComponent } from '@taskbuilder/core';
import { baserect } from './baserect.model';
import { ReportObjectType } from './report-object-type.model';


export enum GaugeObjectType { none, arc, linear, radial, wrong }
export enum GaugeObjectStyle { arrow = 1, bar = 2 }

export class gauge extends baserect {
    value: number = 0;
    type: GaugeObjectType;
    max: number;
    min: number;
    minorUnit: number;
    majorUnit: number;
    ranges: range[] = [];
    pointers: pointer[] = [];
    scale = "{ vertical: false }";
    constructor(jsonObj: any) {
        super(jsonObj.baserect);
        this.obj = ReportObjectType.gauge;
        this.value = jsonObj.value;
        this.type = jsonObj.gaugeType;
        this.max = jsonObj.max;
        this.min = jsonObj.min;
        this.minorUnit = jsonObj.minorUnit;
        this.majorUnit = jsonObj.majorUnit;

        if (jsonObj.ranges) {
            jsonObj.ranges.forEach(element => {
                this.ranges.push(new range(element));
            });
        }
        if (jsonObj.pointers) {
            jsonObj.pointers.forEach(element => {
                this.pointers.push(new pointer(element));
            });
        }

        if (jsonObj.vertical) {
            this.scale = "{ vertical:" + jsonObj.vertical + "}";
        }
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

export class pointer {
    transparent: number;
    color: string;
    style: GaugeObjectStyle;
    value: number;

    constructor(jsonObj: any) {
        this.transparent = jsonObj.transparent;
        this.color = jsonObj.color;
        this.style = jsonObj.style;
        this.value = jsonObj.value ? jsonObj.value : 0;
    }

    getStyleName(): string {
        switch (this.style) {
            case GaugeObjectStyle.arrow:
                return 'arrow';
            case GaugeObjectStyle.bar:
                return 'bar';
            default:
                return 'arrow';
        }
    }
}
