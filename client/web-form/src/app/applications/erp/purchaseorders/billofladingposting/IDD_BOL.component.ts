import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_BOLService } from './IDD_BOL.service';

@Component({
    selector: 'tb-IDD_BOL',
    templateUrl: './IDD_BOL.component.html',
    providers: [IDD_BOLService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_BOLComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_BOL_RSN_SPECIFICATOR_TYPE_1_itemSource: any;
public IDC_BOL_RSN_SPECIFICATOR_TYPE_2_itemSource: any;
public IDC_BOL_DETAIL_BE_UOM_itemSource: any;

    constructor(document: IDD_BOLService,
        eventData: EventDataService,
        resolver: ComponentFactoryResolver,
        private store: Store,
        ciService: ComponentInfoService,
        changeDetectorRef: ChangeDetectorRef) {
		super(document, eventData, ciService, changeDetectorRef, resolver);
        this.eventData.change.subscribe(() => this.changeDetectorRef.detectChanges());
    }

    ngOnInit() {
        super.ngOnInit();
        this.IDC_BOL_RSN_SPECIFICATOR_TYPE_1_itemSource = {
  "name": "Specificator1TypeCombo",
  "namespace": "ERP.Inventory.Components.SpecTypeNoIgnoreCombo"
}; 
this.IDC_BOL_RSN_SPECIFICATOR_TYPE_2_itemSource = {
  "name": "Specificator2TypeCombo",
  "namespace": "ERP.Inventory.Components.SpecTypeNoIgnoreCombo"
}; 
this.IDC_BOL_DETAIL_BE_UOM_itemSource = {
  "name": "UnitsOfMeasureFromItemComboBox",
  "namespace": "ERP.PurchaseOrders.Documents.UnitsOfMeasureFromItemComboBox"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'BillOfLadingPosting':['InvRsn','CustSupp','DocNo','DocumentDate','PreprintedDocNo','PostingDate','StoragePhase1','Specificator1Type','SpecificatorPhase1','StoragePhase2','Specificator2Type','SpecificatorPhase2'],'HKLInvEntr':['Description'],'HKLSupplier':['CompNameCompleteWithTaxNumber'],'HKLStorageF1':['Description'],'HKLSpecificatorF1':['CompanyName'],'HKLStorageF2':['Description'],'HKLSpecificatorF2':['CompanyName'],'global':['Detail'],'Detail':['PurchaseOrdNo','Position','Drawing','Item','Descri','UoM','Qty','UnitValue','DiscountFormula','DiscountAmount','LineAmount','Department','Lot','NoOfPacks','Closed','SaleOrdNo','SaleOrdPos','Job','CostCenter','InvEntryDe_CostAccAccount'],'HKLCostAccAccount':['Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_BOLFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_BOLComponent, resolver);
    }
} 