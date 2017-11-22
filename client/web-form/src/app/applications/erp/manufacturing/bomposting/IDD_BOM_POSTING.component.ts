import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_BOM_POSTINGService } from './IDD_BOM_POSTING.service';

@Component({
    selector: 'tb-IDD_BOM_POSTING',
    templateUrl: './IDD_BOM_POSTING.component.html',
    providers: [IDD_BOM_POSTINGService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_BOM_POSTINGComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_BOM_POSTING_COMPONENT_TYPE_itemSource: any;
public IDC_BOM_POSTING_BE_CODETYPE_SPECIFICATOR_itemSource: any;
public IDC_BOM_POSTING_BE_CODETYPEVALUATE_itemSource: any;
public IDC_BOM_POSTING_BE_OP_ITEM_itemSource: any;
public IDC_BOM_POSTING_CODETYPE_SPEC_ISSUETOPROD_itemSource: any;
public IDC_BOM_POSTING_CODETYPE_SPEC_ISSUETOPROD_SCRAP_itemSource: any;
public IDC_BOM_POSTING_CODETYPE_SPEC_ISSUETOPROD_SECOND_RATE_itemSource: any;
public IDC_BOM_POSTING_CODETYPE_SPEC_ISSUE_itemSource: any;

    constructor(document: IDD_BOM_POSTINGService,
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
        this.IDC_BOM_POSTING_COMPONENT_TYPE_itemSource = {
  "name": "BOMPostComponentTypeCombo",
  "namespace": "ERP.Manufacturing.Documents.BOMPostingItemTypeItemSource"
}; 
this.IDC_BOM_POSTING_BE_CODETYPE_SPECIFICATOR_itemSource = {
  "name": "SpecificatorTypeCombo",
  "namespace": "ERP.Inventory.Components.SpecTypeNoIgnoreCombo"
}; 
this.IDC_BOM_POSTING_BE_CODETYPEVALUATE_itemSource = {
  "name": "InventoryValueCriteriaCombo",
  "namespace": "ERP.Manufacturing.Documents.BOMPostingEnumCombo"
}; 
this.IDC_BOM_POSTING_BE_OP_ITEM_itemSource = {
  "name": "BOMComboBox",
  "namespace": "ERP.Manufacturing.Documents.FPComboBox"
}; 
this.IDC_BOM_POSTING_CODETYPE_SPEC_ISSUETOPROD_itemSource = {
  "name": "IssueSpecificatorTypeCombo",
  "namespace": "ERP.Inventory.Components.SpecTypeNoIgnoreCombo"
}; 
this.IDC_BOM_POSTING_CODETYPE_SPEC_ISSUETOPROD_SCRAP_itemSource = {
  "name": "ScrapIssueSpecificatorTypeCombo",
  "namespace": "itemsource.erp.inventory.components.spectypenoignorecombo"
}; 
this.IDC_BOM_POSTING_CODETYPE_SPEC_ISSUETOPROD_SECOND_RATE_itemSource = {
  "name": "SecondRateIssueSpecificatorTypeCombo",
  "namespace": "itemsource.erp.inventory.components.spectypenoignorecombo"
}; 
this.IDC_BOM_POSTING_CODETYPE_SPEC_ISSUE_itemSource = {
  "name": "ClearingSpecificatorTypeCombo",
  "namespace": "itemsource.erp.inventory.components.spectypenoignorecombo"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['Item','Variant','Quantity','Level','bUseFPProductionCosts','bUseSFLoadsProductionCosts','bUseSFUnloadsProductionCosts','AlsoMatRequirements','SelectSemifinished','NoLastLevelSemifinished','bStorageChange','BOMPostingComponents','BOMPostingOperations','DocNo','Job','DocDate','PostDate','ItemDataBookInventoryActual','ItemDataBookInvDelta','ItemDataFinalBookInv','ItemDataBookInvDelta','ItemDataFinalBookInv','ItemDataBookInvValue','ItemDataOnHandActual','ItemDataDeltaOnHand','ItemDataFinalOnHand','ItemDataDeltaOnHand','ItemDataFinalOnHand','ItemDataOrderedSupplierActual','ItemDataOrderedSupplierDelta','ItemDataOrderedSupplierIsFinal','ItemDataOrderedSupplierDelta','ItemDataOrderedSupplierIsFinal','ItemDataActInProduction','ItemDataDeltaInProduction','ItemDataFinInProduction','ItemDataDeltaInProduction','ItemDataFinInProduction','ItemDataActCustReserved','ItemDataDeltaCustReserved','ItemDataFinCustReserved','ItemDataDeltaCustReserved','ItemDataFinCustReserved','ItemDataActualAllocated','ItemDataDeltaAllocated','ItemDataFinalAllocated','ItemDataDeltaAllocated','ItemDataFinalAllocated','ItemDataActReservedByProd','ItemDataDeltaReservedByProd','ItemDataFinReservedByProd','ItemDataDeltaReservedByProd','ItemDataFinReservedByProd','ItemDataActualAvailaibility','ItemDataFinalAvailaibility','ItemDataFinalAvailaibility','ItemDataKind','ItemDataValueWith','ItemDataNetWeight','ItemDataGrossWeight','ItemDataMinimumStock','ItemDataMaxStock','ItemDataReorderingLot','ItemDataProductionLot','ItemDataItemPicture','ItemDataStorage','ItemDataStorageBookInventoryActual','ItemDataStorageBookInvDelta','ItemDataStorageBookInvFinal','ItemDataStorageBookInvDelta','ItemDataStorageBookInvFinal','ItemDataStorageBookInvValue','ItemDataActualStorageQty','ItemDataDeltaStorageQty','ItemDataFinalStorageQty','ItemDataDeltaStorageQty','ItemDataFinalStorageQty','ItemDataActualSupplierStorageOrdered','ItemDataDeltaSupplierStorageOrdered','ItemDataFinalSupplierStorageOrdered','ItemDataDeltaSupplierStorageOrdered','ItemDataFinalSupplierStorageOrdered','ItemDataStorageProdInvActual','ItemDataStorageProdInvDelta','ItemDataStorageProdInvFinal','ItemDataStorageProdInvDelta','ItemDataStorageProdInvFinal','ItemDataActCustStorageReserved','ItemDataDeltaCustStorageReserved','ItemDataFinCustStorageReserved','ItemDataDeltaCustStorageReserved','ItemDataFinCustStorageReserved','ItemDataActualStorageAllocated','ItemDataDeltaStorageAllocated','ItemDataFinalStorageAllocated','ItemDataDeltaStorageAllocated','ItemDataFinalStorageAllocated','ItemDataStorageActReservedByProd','ItemDataStorageDeltaReservedByProd','ItemDataStorageFinReservedByProd','ItemDataStorageDeltaReservedByProd','ItemDataStorageFinReservedByProd','ItemDataActualStorageAvailaibility','ItemDataFinalStorageAvailaibility','ItemDataFinalStorageAvailaibility','ItemDataStorageMinimumStock','ItemDataStorageMaximumStock','ItemDataOnHandActual','ItemDataFinalOnHand','ItemDataMinimumStock','AvailabilityAnalysisDetails','LifoFifoHistory','ManItemDataDrawing','ManItemDataInProcessBOM','ManItemDataNotes','ManItemDataMRPPolicy','ManItemDataLeadTime','ManItemDataOrderReleaseDays','ManItemDataRMCost','ManItemDataSetupCost','ManItemDataInHouseProcessingCost','ManItemDataOutsourcedProcessingCost','ManItemDataProductionCost','ManItemDataProductionCostLastChange','ManItemDataItemData_LastCost','ManItemDataMO','ManItemDataStandardCost','DBTManItemDataManufacturing','ManItemDatabTreeView','DBTManItemDataTechnicalData','WMSItemDataStorage','WMSItemDataForPutawayByZone','WMSItemDataForPickingByZone','WMSItemDataBlockedByZone','DBTWMSItemDataZone','WMSItemDataForPutawayByBin','WMSItemDataForPickingByBin','WMSItemDataBlockedByBin','DBTWMSItemDataBin','WMSItemDataForPutawayByStock','WMSItemDataForPickingByStock','WMSItemDataBlockedByStock','DBTWMSItemDataStocks','WMSItemDataCrossDockingTotal','DBTWMSItemDataCrossDocking','IssueReason','IssueStorage','IssueSpecificatorType','IssueSpecificator','ScrapInvReason','ScrapIssueStorage','ScrapIssueSpecificatorType','ScrapIssueSpecificator','SecondRateInvReason','SecondRateIssueStorage','SecondRateIssueSpecificatorType','SecondRateIssueSpecificator','ClearingReason','ClearingStorage','ClearingSpecificatorType','ClearingSpecificator','LoadPic','UnloadPic'],'BOMPostingComponents':['Bmp','Selected','Line','BOMLevel','BOMPostComponentType','Component','ComponentVariant','UoM','BOMPostStorage','BOMPostSpecificatorType','BOMPostSpecificator','BOMPostInvEntryQty','OnHandStorageQty','BOMPostRemainingQty','BOMPostOnHandTot','Lot','ComponentDescription','BOMPostAccountingType','InventoryValueCriteria','EvaluateByLot','Cost','InhouseProcessingCost','OutsourcedProcessingCost'],'BOMPostingOperations':['Selected','BOM','ComponentVariant','ComponentDescription','Line','Operation','WC','Cost'],'HKLOperation':['Description'],'HKLWC':['Description'],'AvailabilityAnalysisDetails':['FromDate','IssuedQuantityBaseUoM','PickedQuantityBaseUoM','ProgrBalanceQty','CodeType','DocumentNumber','Line','CustSupp','CompanyName','Job','UoM','DocQty'],'LifoFifoHistory':['ReceiptBatchId','Storage','AccountingType','InvEntryType','PostingDate','Qty','LineCost'],'DBTManItemDataManufacturing':['DeliveryDate','MONo','UoM','ProductionQty','MOStatus'],'DBTManItemDataTechnicalData':['Name','NumberValue','StringValue','DateValue','BoolValue','Notes'],'DBTWMSItemDataZone':['Zone','ZoneDescri','ConsignmentPartner','AvailableQty','SumQtyBaseUoM','SumQtyReserved','SumQtyIncoming','SumQtyIsMultilevelSU','ForPicking','ForPutaway','Blocked','SpecialStock'],'DBTWMSItemDataBin':['Zone','ZoneDescri','Bin','ConsignmentPartner','AvailableQty','SumQtyBaseUoM','SumQtyReserved','SumQtyIncoming','SumQtyIsMultilevelSU','ForPicking','ForPutaway','Blocked','SpecialStock'],'DBTWMSItemDataStocks':['Zone','ZoneDescri','Bin','Lot','LotValidTo','SpecialStock','ConsignmentPartner','AvailableQty','SumQtyBaseUoM','SumQtyReserved','SumQtyIncoming','SumQtyIsMultilevelSU','UnitOfMeasure','SumQty','ForPicking','ForPutaway','Blocked'],'DBTWMSItemDataCrossDocking':['Lot','ConsignmentPartner','QtyBaseUoM'],'HKLIssueReason':['Description'],'HKLScrapInvReason':['Description'],'HKLSecondRateInvReason':['Description'],'HKLClearingReason':['Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_BOM_POSTINGFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_BOM_POSTINGComponent, resolver);
    }
} 