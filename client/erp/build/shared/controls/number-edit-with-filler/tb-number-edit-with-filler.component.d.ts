import { TbComponentService, LayoutService, EventDataService, ControlComponent } from '@taskbuilder/core';
import { ChangeDetectorRef } from '@angular/core';
export declare class NumberEditWithFillerComponent extends ControlComponent {
    eventData: EventDataService;
    enableFilling: boolean;
    maxLength: number;
    fillerDigit: string;
    minLen: number;
    errorMessage: string;
    constructor(eventData: EventDataService, layoutService: LayoutService, tbComponentService: TbComponentService, changeDetectorRef: ChangeDetectorRef);
    onPasting(e: ClipboardEvent): void;
    onTyping(e: KeyboardEvent): void;
    changeModelValue(value: any): void;
    onBlur(): void;
}
