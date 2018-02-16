import { TbComponentService, LayoutService, ControlComponent, EventDataService, Store } from '@taskbuilder/core';
import { WmsHttpService } from '../../../core/services/wms/wms-http.service';
import { ChangeDetectorRef } from '@angular/core';
export declare class StrBinEditComponent extends ControlComponent {
    eventData: EventDataService;
    private http;
    private store;
    slice: any;
    selector: any;
    mask: string;
    errorMessage: string;
    constructor(eventData: EventDataService, layoutService: LayoutService, tbComponentService: TbComponentService, changeDetectorRef: ChangeDetectorRef, http: WmsHttpService, store: Store);
    ngOnChanges(changes: any): Promise<void>;
    private convertMask(mask, separator, maskChar);
    changeModelValue(value: any): void;
    onKeyDown($event: any): void;
}
