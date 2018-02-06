import { ViewContainerRef, ChangeDetectorRef } from '@angular/core';
import { Store, ControlComponent, TbComponentService, LayoutService, ParameterService } from '@taskbuilder/core';
import { ItemsHttpService } from '../../../core/services/items/items-http.service';
export declare class ItemEditComponent extends ControlComponent {
    vcr: ViewContainerRef;
    private store;
    private http;
    private parameterService;
    slice: any;
    selector: any;
    hotLink: {
        namespace: string;
        name: string;
    };
    maxLength: number;
    itemsAutoNumbering: boolean;
    constructor(vcr: ViewContainerRef, layoutService: LayoutService, tbComponentService: TbComponentService, changeDetectorRef: ChangeDetectorRef, store: Store, http: ItemsHttpService, parameterService: ParameterService);
    ngOnInit(): void;
    readParams(): Promise<void>;
}
