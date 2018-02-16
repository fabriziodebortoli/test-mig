import { TbComponentService, LayoutService, EventDataService, ControlComponent } from '@taskbuilder/core';
import { OnInit, ChangeDetectorRef } from '@angular/core';
export declare class ChartOfAccountComponent extends ControlComponent implements OnInit {
    eventData: EventDataService;
    hotLink: {
        namespace: string;
        name: string;
    };
    errorMessage: string;
    selector: any;
    slice$: any;
    constructor(eventData: EventDataService, layoutService: LayoutService, tbComponentService: TbComponentService, changeDetectorRef: ChangeDetectorRef);
    ngOnInit(): void;
    changeModelValue(value: any): void;
    onBlur(): void;
}
