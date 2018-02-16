import { FormMode, ContextMenuItem, Store, TbComponentService, LayoutService, ControlComponent, EventDataService } from '@taskbuilder/core';
import { DataService, HttpService, ParameterService } from '@taskbuilder/core';
import { ChangeDetectorRef, OnInit, OnChanges } from '@angular/core';
import { CoreHttpService } from '../../../core/services/core/core-http.service';
import { Http } from '@angular/http';
export declare class TaxIdEditComponent extends ControlComponent implements OnInit, OnChanges {
    private eventData;
    private dataService;
    private parameterService;
    private store;
    private httpservice;
    private httpCore;
    private http;
    readonly: boolean;
    slice: any;
    selector: any;
    errorMessage: any;
    private ctrlEnabled;
    private isMasterBR;
    private isMasterIT;
    private isMasterRO;
    private isEuropeanUnion;
    private naturalPerson;
    private isoCode;
    checktaxidcodeContextMenu: ContextMenuItem[];
    menuItemITCheck: ContextMenuItem;
    menuItemEUCheck: ContextMenuItem;
    menuItemBRCheck: ContextMenuItem;
    menuItemROCheck: ContextMenuItem;
    constructor(layoutService: LayoutService, eventData: EventDataService, dataService: DataService, tbComponentService: TbComponentService, changeDetectorRef: ChangeDetectorRef, parameterService: ParameterService, store: Store, httpservice: HttpService, httpCore: CoreHttpService, http: Http);
    ngOnChanges(changes: any): void;
    ngOnInit(): void;
    openMessageDialog(message: string): Promise<any>;
    onFormModeChanged(formMode: FormMode): void;
    buildContextMenu(): void;
    isTaxIdField(code: string, all: boolean): boolean;
    onBlur(): Promise<void>;
    changeModelValue(value: any): void;
    checkIT(): Promise<void>;
    checkBR(): Promise<void>;
    checkRO(): Promise<void>;
    checkEU(): Promise<void>;
    fillFields(result: any): Promise<void>;
    getStato(): Promise<string>;
    validate(): void;
    isTaxIdNumber(fiscalcode: string): boolean;
    readonly isValid: boolean;
}
