import { OnChanges, AfterViewInit } from '@angular/core';
import { ControlComponent } from './../control.component';
import { EventDataService } from './../../services/eventdata.service';
export declare class DateInputComponent extends ControlComponent implements OnChanges, AfterViewInit {
    private eventData;
    forCmpID: string;
    formatter: string;
    readonly: boolean;
    selectedDate: Date;
    dateFormat: string;
    constructor(eventData: EventDataService);
    onChange(val: any): void;
    onBlur(): void;
    onUpdateNgModel(newDate: Date): void;
    ngAfterViewInit(): void;
    ngOnChanges(): void;
    getFormat(formatter: string): string;
    modelValid(): boolean;
}
