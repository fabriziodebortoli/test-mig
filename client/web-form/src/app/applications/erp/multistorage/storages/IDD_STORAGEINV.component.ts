import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_STORAGEINVService } from './IDD_STORAGEINV.service';

@Component({
    selector: 'tb-IDD_STORAGEINV',
    templateUrl: './IDD_STORAGEINV.component.html',
    providers: [IDD_STORAGEINVService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_STORAGEINVComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_STORAGEINV_COUNTY_itemSource: any;
public IDC_STORAGEINV_STATUS_itemSource: any;
public IDC_STORAGE_OFFSETS_SYMBOL_itemSource: any;

    constructor(document: IDD_STORAGEINVService,
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
        this.IDC_STORAGEINV_COUNTY_itemSource = {
  "name": "ItemSourceXml",
  "namespace": "ItemSource.Framework.TbGes.TbGes.ItemSourceXml",
  "parameter": "DataFile.ERP.Company.County"
}; 
this.IDC_STORAGEINV_STATUS_itemSource = {
  "name": "ItemSourceXml",
  "namespace": "ItemSource.Framework.TbGes.TbGes.ItemSourceXml",
  "parameter": "DataFile.ERP.Company.State"
}; 
this.IDC_STORAGE_OFFSETS_SYMBOL_itemSource = {
  "name": "ItemSourceXml",
  "namespace": "ItemSource.Framework.TbGes.TbGes.ItemSourceXml",
  "parameter": "DataFile.ERP.InventoryAccounting.OffsetSymbols",
  "useProductLanguage": true
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'Storages':['Storage','Disabled','Description','GroupCode','OwnedGoods','WMS','ExcludeFromOnHand','ExcludeFromValuation','Notes','CodeType','WAPTransferBetweenStorages','ISOCountryCode','ZIPCode','Address','Address2','StreetNo','FederalState','City','District','Telephone1','Telephone2','Fax','ISOCountryCode','Address','Address2','City','ZIPCode','County','Country','Telephone1','Telephone2','Fax','StubBookSales','StubBookPurchases','StubBookAdjustment','StubBookInterStorageIN','StubBookInterStorageOUT','TaxJournalSales','TaxJournalPurchases','SalesShortageCheckType','SalesScarcityCheckType','SalesOrdersShortageCheckType','InventoryShortageCheckType','InventoryScarcityCheckType','Priority','UsedByProduction','UsedByMRP','UsedForRetail','StorageReplenishment','SpecTypeReplenishment','SpecificatorReplenishment','ManufacturingIssuePickZone','ManufacturingIssueZone','ManTwoStepsPutaway','ManufacturingReceiptZone','WMSActivationDate','WMSManActivationDate','ConsignmentStock','StorageBarcodePrefix','LastSnapshotCertifiedDate','SearchZoneStrategyPutaway','SearchZoneStrategyPicking','StockReturnStrategy','TwoStepsPutaway','GoodsReceiptZone','GoodsIssueZone','ReturnZone','ScrapZone','InspectionZone','CrossDocking','CrossDockingZone'],'HKLGroups':['Description'],'HKLStubBookSales':['Description'],'HKLStubBookPurchases':['Description'],'HKLStubBookAdjustment':['Description'],'HKLStubBookInterStorageIN':['Description'],'HKLStubBookInterStorageOUT':['Description'],'HKLTaxJournalSales':['Description'],'HKLTaxJournalPurchases':['Description'],'HKLStorageReplenishment':['Description'],'HKLSpecificatorReplenishment':['CompanyName'],'global':['StoragesManWMS','Offsets','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'StoragesManWMS':['StorageChild'],'HKLStorages':['Description'],'HKLMIPZone':['Description'],'HKLMIZone':['Description'],'HKLMRZone':['Description'],'HKLSearchZonePutaway':['Description'],'HKLSearchZonePicking':['Description'],'HKLGRZone':['Description'],'HKLGIZone':['Description'],'HKLReturnZone':['Description'],'HKLScrapZone':['Description'],'HKLInspectionZone':['Description'],'HKLCrossDocking':['Description'],'OffsetsStoragesHead':['PurchaseOffset','SaleOffset','ConsumptionOffset','CustomerAccountRoot','SalesGoodsOffset','ServicesSalesOffset','SupplierAccountRoot','PurchasesGoodsOffset','ServicesPurchasesOffset'],'HKLPurchaseOffset':['Description'],'HKLSaleOffset':['Description'],'HKLConsumptionOffset':['Description'],'HKLCustomerAccountRoot':['Description'],'HKLSalesGoodsOffset':['Description'],'HKLServicesSalesOffset':['Description'],'HKLSupplierAccountRoot':['Description'],'HKLPurchasesGoodsOffset':['Description'],'HKLServicesPurchasesOffset':['Description'],'Offsets':['OffsetSymbol','OffsetSymbolDescription','Offset'],'HKLAccount':['Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_STORAGEINVFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_STORAGEINVComponent, resolver);
    }
} 