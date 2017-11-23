import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_TS_EXPORTCSVFILEService } from './IDD_TS_EXPORTCSVFILE.service';

@Component({
    selector: 'tb-IDD_TS_EXPORTCSVFILE',
    templateUrl: './IDD_TS_EXPORTCSVFILE.component.html',
    providers: [IDD_TS_EXPORTCSVFILEService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_TS_EXPORTCSVFILEComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_TS_EXPORT_CSV_DOCTYPE_itemSource: any;
public IDC_TS_EXPORT_CSV_FILE_BE_OPERATION_TYPE_itemSource: any;

    constructor(document: IDD_TS_EXPORTCSVFILEService,
        eventData: EventDataService,
        resolver: ComponentFactoryResolver,
        private store: Store,
        ciService: ComponentInfoService,
        changeDetectorRef: ChangeDetectorRef) {
		super(document, eventData, ciService, changeDetectorRef, resolver);
        this.subscriptions.push(this.eventData.change.subscribe(() => changeDetectorRef.detectChanges()));
    }

    ngOnInit() {
        super.ngOnInit();
        this.IDC_TS_EXPORT_CSV_DOCTYPE_itemSource = {
  "name": "SalesDocAccEnumCombo",
  "namespace": "ERP.TESANConnector.Components.SalesDocAccEnumCombo"
}; 
this.IDC_TS_EXPORT_CSV_FILE_BE_OPERATION_TYPE_itemSource = {
  "name": "TSOperationTypeCombo",
  "namespace": "ERP.TESANConnector.Components.TSOperationTypeCombo"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['Year','bDocTypeAll','bDocTypeSel','DocType','bDocDateAll','bDocDateSel','FromDocDate','ToDocDate','bDocNoAll','bDocNoSel','FromDocNo','ToDocNo','bJobAll','bJobSel','FromJob','ToJob','TSExportCsvFileDetail'],'TSExportCsvFileDetail':['TEnhTSExp_bSelected','SaleDocId','DocumentType','DocumentDate','DocNo','TEnhTSExp_Year','TEnhTSExp_TSChargeType','TEnhTSExp_TSChargeTypeDesc','TEnhTSExp_TSChargeTypeFlag','TEnhTSExp_TSChargeTypeFlagDesc','TEnhTSExp_AmountToTransfer','TEnhTSExp_TransferredAmount','TEnhTSExp_OperationType','TEnhTSExp_InstallmentDate']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_TS_EXPORTCSVFILEFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_TS_EXPORTCSVFILEComponent, resolver);
    }
} 