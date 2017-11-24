import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_BARCODESTRUCTUREService } from './IDD_BARCODESTRUCTURE.service';

@Component({
    selector: 'tb-IDD_BARCODESTRUCTURE',
    templateUrl: './IDD_BARCODESTRUCTURE.component.html',
    providers: [IDD_BARCODESTRUCTUREService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_BARCODESTRUCTUREComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_BARCODESTRUCTURE_TYPE_itemSource: any;
public IDC_BARCODESTRUCTURE_DETAIL_DATA_itemSource: any;

    constructor(document: IDD_BARCODESTRUCTUREService,
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
        this.IDC_BARCODESTRUCTURE_TYPE_itemSource = {
  "allowChanges": false,
  "name": "BarcodeTypeCombo",
  "namespace": "ERP.Barcode.Documents.BarcodeTypeEnumCombo",
  "useProductLanguage": false
}; 
this.IDC_BARCODESTRUCTURE_DETAIL_DATA_itemSource = {
  "name": "DataEnumCombo",
  "namespace": "ERP.Barcode.Documents.BarCodeDataEnumCombo"
}; 

        		this.bo.appendToModelStructure({'DBTBarcodeStructure':['Code','Disabled','Description','Prefix','BarcodeType'],'global':['DBTBarcodeStructureDetails','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'DBTBarcodeStructureDetails':['Data','InitialSeparator','Prefix','FinalSeparator','Length','Position']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_BARCODESTRUCTUREFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_BARCODESTRUCTUREComponent, resolver);
    }
} 