import { OnChanges, AfterViewInit } from '@angular/core';
import { ControlComponent } from './../control.component';
import { EventDataService } from './../../services/eventdata.service';
export declare class NumericTextBoxComponent extends ControlComponent implements OnChanges, AfterViewInit {
    private eventData;
    forCmpID: string;
    formatter: string;
    disabled: boolean;
    decimals: number;
    width: number;
    private errorMessage;
    private constraint;
    selectedValue: number;
    private showError;
    formatOptionsCurrency: any;
    formatOptionsInteger: any;
    formatOptionsDouble: string;
    formatOptionsPercent: any;
    constructor(eventData: EventDataService);
    getDecimalsOptions(): number;
    getFormatOptions(): any;
    onChange(val: any): void;
    onUpdateNgModel(newValue: number): void;
    ngAfterViewInit(): void;
    ngOnChanges(): void;
    modelValid(): boolean;
    onBlur(): any;
}
