import { FormMode, ContextMenuItem, Store, TbComponentService, LayoutService, ControlComponent, EventDataService, HttpService, ParameterService } from '@taskbuilder/core';
import { ChangeDetectorRef, OnInit, OnChanges } from '@angular/core';
import { CoreHttpService } from '../../../core/services/core/core-http.service';
import { Http } from '@angular/http';
export declare class FiscalCodeEditComponent extends ControlComponent implements OnInit, OnChanges {
    private eventData;
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
    private isEuropeanUnion;
    private isoCode;
    checkfiscalcodeContextMenu: ContextMenuItem[];
    menuItemITCheck: ContextMenuItem;
    menuItemEUCheck: ContextMenuItem;
    menuItemBRCheck: ContextMenuItem;
    constructor(layoutService: LayoutService, eventData: EventDataService, tbComponentService: TbComponentService, changeDetectorRef: ChangeDetectorRef, parameterService: ParameterService, store: Store, httpservice: HttpService, httpCore: CoreHttpService, http: Http);
    ngOnInit(): void;
    ngOnChanges(changes: any): void;
    onFormModeChanged(formMode: FormMode): void;
    buildContextMenu(): void;
    isTaxIdField(code: string, all: boolean): boolean;
    onBlur(): Promise<void>;
    changeModelValue(value: any): void;
    checkIT(): Promise<void>;
    checkBR(): Promise<void>;
    checkEU(): Promise<void>;
    getStato(): Promise<string>;
    validate(): void;
    isTaxIdNumber(fiscalcode: string): boolean;
    isFiscalCodeValid(fiscalcode: string, country: string): void;
    isSupported(country: string): boolean;
    FiscalCodeCheckIT(fiscalcode: string): void;
    FiscalCodeCheckBR(fiscalcode: string): void;
    FiscalCodeCheckES(fiscalcode: string): void;
    readonly isValid: boolean;
}