import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_SALE_ORD_FULFILLMENTService } from './IDD_SALE_ORD_FULFILLMENT.service';

@Component({
    selector: 'tb-IDD_SALE_ORD_FULFILLMENT',
    templateUrl: './IDD_SALE_ORD_FULFILLMENT.component.html',
    providers: [IDD_SALE_ORD_FULFILLMENTService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_SALE_ORD_FULFILLMENTComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_ELABSALEORD_DOCTYPE_itemSource: any;
public IDC_ELABSALEORD_CODETYPE_SPECIFICATOR_itemSource: any;

    constructor(document: IDD_SALE_ORD_FULFILLMENTService,
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
        this.IDC_ELABSALEORD_DOCTYPE_itemSource = {
  "name": "DocumentTypeCombo",
  "namespace": "ERP.Sales.Components.SaleDocTypeForSaleOrdFulfillmentCombo"
}; 
this.IDC_ELABSALEORD_CODETYPE_SPECIFICATOR_itemSource = {
  "name": "SpecificatorTypeCombo",
  "namespace": "ERP.Inventory.Components.SpecTypeNoIgnoreCombo"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['DocumentType','bOnlyDeliveredOrd','bOnHandCheck','bGeneralInventory','bSelectedStorage','bAllStorages','bStorageSel','Storage','SpecificatorType','Specificator','bPriority','bExpectedDelivery','bMinimumStock','bInsufficiency','FromExpectedDelivery','ToExpectedDelivery','StartingDate','EndingDate','FromConfirmedDelivery','ToConfirmedDelivery','AllSaleOrdNo','SaleOrdNoSel','FromSaleOrdNo','ToSaleOrdNo','AllCustomer','CustomerSel','FromCustomer','ToCustomer','AllPriority','PrioritySel','FromPriority','ToPriority','InvAllCustomer','InvCustomerSel','FromInvCustomer','ToInvCustomer','AllItems','ItemSel','FromItem','ToItem','AllSalesPeople','SalesPeopleSel','FromSalesperson','ToSalesperson','AllProductionJob','ProductionJobSel','FromProducitonJob','ToProductionJob','SaleOrdFulfilmentDetail','OperationDate','bPrintMail','eMailAddressType','bPrintPostaLite','nCurrentElement','GaugeDescription','ProgressViewer'],'SaleOrdFulfilmentDetail':['Bmp','DocGeneration','InternalOrdNo','ExternalOrdNo','Priority','OrderDate','ExpectedDeliveryDate','ConfirmedDeliveryDate','BlockedCust','Customer','CustomerCompanyName','Payment','Currency','TaxJournal','StubBook','Carrier1','SendDocumentsTo','ShipToAddress','Port','Package','Transport','Salesperson','Area','InvRsn','Job','CostCenter'],'HKLCarrierBE':['CompNameComplete'],'ProgressViewer':['TEnhProgressViewer_P1','TEnhProgressViewer_P2','TEnhProgressViewer_P3']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_SALE_ORD_FULFILLMENTFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_SALE_ORD_FULFILLMENTComponent, resolver);
    }
} 