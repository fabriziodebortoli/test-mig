import { TbComponentService, LayoutService, EventDataService, ControlComponent } from '@taskbuilder/core';
import { ChangeDetectorRef } from '@angular/core';
export declare class EsrComponent extends ControlComponent {
    eventData: EventDataService;
    minLen: number;
    errorMessage: string;
    constructor(eventData: EventDataService, layoutService: LayoutService, tbComponentService: TbComponentService, changeDetectorRef: ChangeDetectorRef);
    onPasting(e: ClipboardEvent): void;
    onTyping(e: KeyboardEvent): void;
    changeModelValue(value: any): void;
    onBlur(): void;
    validate(): void;
    readonly isValid: boolean;
}
