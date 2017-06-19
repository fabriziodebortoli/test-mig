import { OnInit } from '@angular/core';
import { ControlComponent } from './../../control.component';
import { EventDataService } from './../../../services/eventdata.service';
export declare class LinearGaugeComponent extends ControlComponent implements OnInit {
    private eventData;
    maxRange: number;
    bandColor: string;
    bandOpacity: number;
    rulerAxis: any;
    ngOnInit(): void;
    constructor(eventData: EventDataService);
    setDefault(): void;
    onBlur(): void;
}
