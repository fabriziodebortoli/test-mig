import { OnChanges, AfterViewInit } from '@angular/core';
import { ControlComponent } from './../control.component';
import { EventDataService } from './../../services/eventdata.service';
export declare class TimeInputComponent extends ControlComponent implements OnChanges, AfterViewInit {
    private eventData;
    selectedTime: number;
    forCmpID: string;
    formatter: string;
    constructor(eventData: EventDataService);
    onChange(val: any): void;
    onUpdateNgModel(newTime: number): void;
    ngAfterViewInit(): void;
    ngOnChanges(): void;
    onBlur(): void;
    modelValid(): boolean;
}
