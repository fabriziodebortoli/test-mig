import { ChangeDetectorRef, OnChanges } from '@angular/core';
import { ControlComponent } from '@taskbuilder/core';
import { EventDataService } from '@taskbuilder/core';
import { TbComponentService } from '@taskbuilder/core';
import { LayoutService } from '@taskbuilder/core';
import { Store } from '@taskbuilder/core';
export declare class NoSpacesEditComponent extends ControlComponent implements OnChanges {
    eventData: EventDataService;
    private store;
    readonly INITIAL_SEGMENT_LENGTH: number;
    readonly: boolean;
    slice: any;
    selector: any;
    hotLink: {
        namespace: string;
        name: string;
    };
    textbox: any;
    errorMessage: string;
    subscribedToSelector: boolean;
    maxLength: number;
    constructor(eventData: EventDataService, layoutService: LayoutService, tbComponentService: TbComponentService, changeDetectorRef: ChangeDetectorRef, store: Store);
    ngOnInit(): void;
    ngOnChanges(changes: any): void;
    subscribeToSelector(): void;
    onKeyDown($event: any): void;
    removeSpaces(): void;
}
