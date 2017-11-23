import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_PROD_DEVService } from './IDD_PROD_DEV.service';

@Component({
    selector: 'tb-IDD_PROD_DEV',
    templateUrl: './IDD_PROD_DEV.component.html',
    providers: [IDD_PROD_DEVService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_PROD_DEVComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_PROD_DEV_MAT_BE_SPEC_TYPE_itemSource: any;

    constructor(document: IDD_PROD_DEVService,
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
        this.IDC_PROD_DEV_MAT_BE_SPEC_TYPE_itemSource = {
  "name": "SpecificatorTypeCombo",
  "namespace": "ERP.Inventory.Components.SpecTypeNoIgnoreCombo"
}; 

        		this.bo.appendToModelStructure({'global':['bContinueToProcess','bMOSel','sMOFrom','sMOTo','bMONotConf','bPlanSel','sPlanNo','bSaleOrdSel','sSaleOrdFrom','sSaleOrdTo','bSelDate','nDateFrom','bJobSel','sJobFrom','sJobTo','bForcePicking','bAlsoMOInProcess','bManageRtgStep','bAutoRunMO','ProducibilityAnalysisSelectedMO','ProducibilityAnalysisMOComponents','ProducibilityAnalysisMOSteps','ProducibilityAnalysisMaterials','nCurrentElement','GaugeDescription','ProgressViewer','BMPGrayFill','BMPGreenFill','BMPYellowFill','BMPGreenEmpty','BMPYellowEmpty','BMPRedFill','BMPGrayFill','BMPGreenFill','BMPYellowFill','BMPRedFill','BMPGrayFill','BMPGreenFill','BMPRedFill','BMPGrayFill','BMPGreenFill','BMPYellowFill','BMPRedFill','BMPFullFill','BMPHalfFill','BMPEmpty','ItemDataBookInventoryActual','ItemDataBookInvDelta','ItemDataFinalBookInv','ItemDataBookInvDelta','ItemDataFinalBookInv','ItemDataBookInvValue','ItemDataOnHandActual','ItemDataDeltaOnHand','ItemDataFinalOnHand','ItemDataDeltaOnHand','ItemDataFinalOnHand','ItemDataOrderedSupplierActual','ItemDataOrderedSupplierDelta','ItemDataOrderedSupplierIsFinal','ItemDataOrderedSupplierDelta','ItemDataOrderedSupplierIsFinal','ItemDataActInProduction','ItemDataDeltaInProduction','ItemDataFinInProduction','ItemDataDeltaInProduction','ItemDataFinInProduction','ItemDataActCustReserved','ItemDataDeltaCustReserved','ItemDataFinCustReserved','ItemDataDeltaCustReserved','ItemDataFinCustReserved','ItemDataActualAllocated','ItemDataDeltaAllocated','ItemDataFinalAllocated','ItemDataDeltaAllocated','ItemDataFinalAllocated','ItemDataActReservedByProd','ItemDataDeltaReservedByProd','ItemDataFinReservedByProd','ItemDataDeltaReservedByProd','ItemDataFinReservedByProd','ItemDataActualAvailaibility','ItemDataFinalAvailaibility','ItemDataFinalAvailaibility','ItemDataKind','ItemDataValueWith','ItemDataNetWeight','ItemDataGrossWeight','ItemDataMinimumStock','ItemDataMaxStock','ItemDataReorderingLot','ItemDataProductionLot','ItemDataItemPicture','ItemDataStorage','ItemDataStorageBookInventoryActual','ItemDataStorageBookInvDelta','ItemDataStorageBookInvFinal','ItemDataStorageBookInvDelta','ItemDataStorageBookInvFinal','ItemDataStorageBookInvValue','ItemDataActualStorageQty','ItemDataDeltaStorageQty','ItemDataFinalStorageQty','ItemDataDeltaStorageQty','ItemDataFinalStorageQty','ItemDataActualSupplierStorageOrdered','ItemDataDeltaSupplierStorageOrdered','ItemDataFinalSupplierStorageOrdered','ItemDataDeltaSupplierStorageOrdered','ItemDataFinalSupplierStorageOrdered','ItemDataStorageProdInvActual','ItemDataStorageProdInvDelta','ItemDataStorageProdInvFinal','ItemDataStorageProdInvDelta','ItemDataStorageProdInvFinal','ItemDataActCustStorageReserved','ItemDataDeltaCustStorageReserved','ItemDataFinCustStorageReserved','ItemDataDeltaCustStorageReserved','ItemDataFinCustStorageReserved','ItemDataActualStorageAllocated','ItemDataDeltaStorageAllocated','ItemDataFinalStorageAllocated','ItemDataDeltaStorageAllocated','ItemDataFinalStorageAllocated','ItemDataStorageActReservedByProd','ItemDataStorageDeltaReservedByProd','ItemDataStorageFinReservedByProd','ItemDataStorageDeltaReservedByProd','ItemDataStorageFinReservedByProd','ItemDataActualStorageAvailaibility','ItemDataFinalStorageAvailaibility','ItemDataFinalStorageAvailaibility','ItemDataStorageMinimumStock','ItemDataStorageMaximumStock','ItemDataOnHandActual','ItemDataFinalOnHand','ItemDataMinimumStock','AvailabilityAnalysisDetails','LifoFifoHistory','ManItemDataDrawing','ManItemDataInProcessBOM','ManItemDataNotes','ManItemDataMRPPolicy','ManItemDataLeadTime','ManItemDataOrderReleaseDays','ManItemDataRMCost','ManItemDataSetupCost','ManItemDataInHouseProcessingCost','ManItemDataOutsourcedProcessingCost','ManItemDataProductionCost','ManItemDataProductionCostLastChange','ManItemDataItemData_LastCost','ManItemDataMO','ManItemDataStandardCost','DBTManItemDataManufacturing','ManItemDatabTreeView','DBTManItemDataTechnicalData','WMSItemDataStorage','WMSItemDataForPutawayByZone','WMSItemDataForPickingByZone','WMSItemDataBlockedByZone','DBTWMSItemDataZone','WMSItemDataForPutawayByBin','WMSItemDataForPickingByBin','WMSItemDataBlockedByBin','DBTWMSItemDataBin','WMSItemDataForPutawayByStock','WMSItemDataForPickingByStock','WMSItemDataBlockedByStock','DBTWMSItemDataStocks','WMSItemDataCrossDockingTotal','DBTWMSItemDataCrossDocking'],'HKLProductionPlan':['Description'],'HKLFromJob':['Description'],'HKLToJob':['Description'],'ProducibilityAnalysisSelectedMO':['FeasibilityCompBmp','MOChildBmp','ToolsBmp','TMO_Selection','Suspended','TMO_Supplier','InHouseOutsourcedBmp','MONo','CreationDate','DeliveryDate','BOM','Variant','TMO_BOMDescri','TMO_StateBmp','MOStatus','UoM','ProductionQty','ProducedQty','Job','CostCenter','Customer','InternalOrdNo','Position'],'ProducibilityAnalysisMOComponents':['FeasibilityCompBmp','Component','Variant','ComponentsDes','UoM','NeededQty','PickedQty','OnHandStorageQty','RequiremetQty','ProductionOrdQty','MONo','SubcntOrdQty','SaleOrdNo','Supplier','SupplierCompanyName','_DeliveryDate','DNRtgStep'],'ProducibilityAnalysisMOSteps':['FeasibilityStepBmp','StepTypeBmp','ToolsBmp','RtgStep','Alternate','AltRtgStep','IsWC','WC','ActualWCDes','ActualWCDes','Operation','OperationDescri','MOStatus','Supplier','SupplierCompanyName'],'ProducibilityAnalysisMaterials':['Sel','Component','Variant','Descri','UoM','NeededQty','PickedQuantity','PickingListNo','Storage','SpecificatorType','Specificator'],'ProgressViewer':['TEnhProgressViewer_P1','TEnhProgressViewer_P2','TEnhProgressViewer_P3'],'AvailabilityAnalysisDetails':['FromDate','IssuedQuantityBaseUoM','PickedQuantityBaseUoM','ProgrBalanceQty','CodeType','DocumentNumber','Line','CustSupp','CompanyName','Job','UoM','DocQty'],'LifoFifoHistory':['ReceiptBatchId','Storage','AccountingType','InvEntryType','PostingDate','Qty','LineCost'],'DBTManItemDataManufacturing':['DeliveryDate','MONo','UoM','ProductionQty','MOStatus'],'DBTManItemDataTechnicalData':['Name','NumberValue','StringValue','DateValue','BoolValue','Notes'],'DBTWMSItemDataZone':['Zone','ZoneDescri','ConsignmentPartner','AvailableQty','SumQtyBaseUoM','SumQtyReserved','SumQtyIncoming','SumQtyIsMultilevelSU','ForPicking','ForPutaway','Blocked','SpecialStock'],'DBTWMSItemDataBin':['Zone','ZoneDescri','Bin','ConsignmentPartner','AvailableQty','SumQtyBaseUoM','SumQtyReserved','SumQtyIncoming','SumQtyIsMultilevelSU','ForPicking','ForPutaway','Blocked','SpecialStock'],'DBTWMSItemDataStocks':['Zone','ZoneDescri','Bin','Lot','LotValidTo','SpecialStock','ConsignmentPartner','AvailableQty','SumQtyBaseUoM','SumQtyReserved','SumQtyIncoming','SumQtyIsMultilevelSU','UnitOfMeasure','SumQty','ForPicking','ForPutaway','Blocked'],'DBTWMSItemDataCrossDocking':['Lot','ConsignmentPartner','QtyBaseUoM']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_PROD_DEVFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_PROD_DEVComponent, resolver);
    }
} 