import { TbComponentService, LayoutService, ControlComponent, EventDataService, Store } from '@taskbuilder/core';
import { ItemsHttpService } from '../../../core/services/items/items-http.service';
import { ChangeDetectorRef } from '@angular/core';
export declare class AutoSearchEditComponent extends ControlComponent {
    eventData: EventDataService;
    private http;
    private store;
    slice: any;
    selector: any;
    items: any[];
    filterText: string;
    constructor(eventData: EventDataService, layoutService: LayoutService, tbComponentService: TbComponentService, changeDetectorRef: ChangeDetectorRef, http: ItemsHttpService, store: Store);
    formatItem(item: any): string;
    ngOnChanges(changes: any): void;
    ngOnInit(): void;
}
