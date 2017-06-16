import { OnInit, OnChanges, AfterViewInit } from '@angular/core';
import { ControlComponent } from './../control.component';
import { EventDataService } from './../../services/eventdata.service';
export declare class LinkComponent extends ControlComponent implements OnInit, OnChanges, AfterViewInit {
    private eventData;
    selectedValue: string;
    pattern: string;
    private constraint;
    private errorMessage;
    private showError;
    constructor(eventData: EventDataService);
    ngOnInit(): void;
    onChange(val: any): void;
    onUpdateNgModel(newValue: string): void;
    ngAfterViewInit(): void;
    ngOnChanges(): void;
    modelValid(): boolean;
    onBlur(): any;
}
