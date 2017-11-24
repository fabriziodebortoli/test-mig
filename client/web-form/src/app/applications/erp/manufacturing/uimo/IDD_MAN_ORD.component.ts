import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_MAN_ORDService } from './IDD_MAN_ORD.service';

@Component({
    selector: 'tb-IDD_MAN_ORD',
    templateUrl: './IDD_MAN_ORD.component.html',
    providers: [IDD_MAN_ORDService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_MAN_ORDComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_MAN_ORD_A_MIS_itemSource: any;
public IDC_ORD_RTGSTEPS_MOCOMP_UOM_itemSource: any;
public IDC_ORD_RTGSTEPS_MOCOMP_RTGSTEP_SENT_itemSource: any;
public IDC_LABOUR_DETAILS_ESTIMATED_LABOUR_TYPE_itemSource: any;
public IDC_TOOLS_MAN_RTGSTEP_itemSource: any;
public IDC_TOOLS_MAN_ALT_itemSource: any;
public IDC_TOOLS_MAN_RTGSTEP_ALT_RTGSTEP_itemSource: any;

    constructor(document: IDD_MAN_ORDService,
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
        this.IDC_MAN_ORD_A_MIS_itemSource = {
  "allowChanges": false,
  "name": "UnitsOfMeasureOrProdComboBox",
  "namespace": "ERP.Manufacturing.Documents.UnitsOfMeasureOrProdComboBox",
  "useProductLanguage": false
}; 
this.IDC_ORD_RTGSTEPS_MOCOMP_UOM_itemSource = {
  "name": "UnitsOfMeasureComponentMOComboBox",
  "namespace": "ERP.Manufacturing.Documents.UnitsOfMeasureComponentMOComboBox"
}; 
this.IDC_ORD_RTGSTEPS_MOCOMP_RTGSTEP_SENT_itemSource = {
  "name": "DNRtgStepIntCombo",
  "namespace": "ERP.Manufacturing.Documents.MORtgStepSentItemSource"
}; 
this.IDC_LABOUR_DETAILS_ESTIMATED_LABOUR_TYPE_itemSource = {
  "name": "eLabourTypeEnumCombo",
  "namespace": "ERP.Routing.Components.LabourLabourTypeItemSource"
}; 
this.IDC_TOOLS_MAN_RTGSTEP_itemSource = {
  "name": "RtgStepIntCombo",
  "namespace": "ERP.ToolsManagement.AddOnsCore.RtgStepToolsComboBox"
}; 
this.IDC_TOOLS_MAN_ALT_itemSource = {
  "name": "AlternateStrCombo",
  "namespace": "ERP.ToolsManagement.AddOnsCore.AlternateToolsItemSource"
}; 
this.IDC_TOOLS_MAN_RTGSTEP_ALT_RTGSTEP_itemSource = {
  "name": "DNRtgStepIntCombo",
  "namespace": "ERP.ToolsManagement.AddOnsCore.AltRtgStepToolsItemSource"
}; 

        		this.bo.appendToModelStructure({'MO':['MONo','ProductionQty','UoM','DeliveryDate','BOM','Variant','ProductionQty','UoM','DeliveryDate','UoM','DeliveryDate','MRPConfirmationRank','Simulation','GroupSF','TMO_ConfirmationLevelDescr','ProductionPlanLine','Job','ProductionLotNumber','CostCenter','Drawing','AccountingType','ProducedQty','ScrapQuantity','SecondRateQuantity','Feasibility','BarcodeSegment','TMO_PickingListNo','Feasibility','BarcodeSegment','CreationDate','LastModificationDate','RunDate','CostsCalculationLastDate','MRPConfirmationRank','Simulation','GroupSF','TMO_ConfirmationLevelDescr','ProductionPlanLine','Printed','PrintDate','Notes','Customer','InternalOrdNo','Position','ECODate','ECORevision','ProcessingBudgetCost','ProcessingActualCost','ProcessingBudgetCost','SetupBudgetCost','LabourBudgetCost','EstimatedMaterialCostkanban','EstimatedProcCostkanban','ProcessingActualCost','SetupActualCost','LabourActualCost','ActualMaterialCostkanban','ActualProcCostkanban','SimStartDate','SimEndDate','EstimatedProcessingTime','EstimatedSetupTime','StartingDate','EndingDate','ActualProcessingTime','ActualSetupTime'],'HKLItem':['Description'],'global':['nMOStatusMax','MOStatusDescription','ImageStatusMO','MOProdDescription','ProdImage','LeftProducedDescription','RightProducedDescription','MOSRDescription','SRImage','LeftSecondRateDescription','RightSecondRateDescription','MOScrapDescription','ScrapImage','LeftScrapDescription','RightScrapDescription','MOComponents','Internal','Outsourced','Supplier','MOSteps','MOSteps','__DBTLabour','DBTToolsManagement','NrSchema','NrSchema','SaleOrdQty','SaleOrdUoM','CustOrdDate','ExpectedMaterialCost','CostTotExpected','ActualMaterialCost','CostTotDeliv','ExpectedMaterialCost','CostTotExpected','CostTotExpectedKanban','ActualMaterialCost','CostTotDeliv','CostTotDelivKanban','MOStepsKanban','MOProcessing','MOHierarchies','MOInventoryEntries','MOLots','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg','RequirementsOrigin','DBTLinksTable','ItemDataBookInventoryActual','ItemDataBookInvDelta','ItemDataFinalBookInv','ItemDataBookInvDelta','ItemDataFinalBookInv','ItemDataBookInvValue','ItemDataOnHandActual','ItemDataDeltaOnHand','ItemDataFinalOnHand','ItemDataDeltaOnHand','ItemDataFinalOnHand','ItemDataOrderedSupplierActual','ItemDataOrderedSupplierDelta','ItemDataOrderedSupplierIsFinal','ItemDataOrderedSupplierDelta','ItemDataOrderedSupplierIsFinal','ItemDataActInProduction','ItemDataDeltaInProduction','ItemDataFinInProduction','ItemDataDeltaInProduction','ItemDataFinInProduction','ItemDataActCustReserved','ItemDataDeltaCustReserved','ItemDataFinCustReserved','ItemDataDeltaCustReserved','ItemDataFinCustReserved','ItemDataActualAllocated','ItemDataDeltaAllocated','ItemDataFinalAllocated','ItemDataDeltaAllocated','ItemDataFinalAllocated','ItemDataActReservedByProd','ItemDataDeltaReservedByProd','ItemDataFinReservedByProd','ItemDataDeltaReservedByProd','ItemDataFinReservedByProd','ItemDataActualAvailaibility','ItemDataFinalAvailaibility','ItemDataFinalAvailaibility','ItemDataKind','ItemDataValueWith','ItemDataNetWeight','ItemDataGrossWeight','ItemDataMinimumStock','ItemDataMaxStock','ItemDataReorderingLot','ItemDataProductionLot','ItemDataItemPicture','ItemDataStorage','ItemDataStorageBookInventoryActual','ItemDataStorageBookInvDelta','ItemDataStorageBookInvFinal','ItemDataStorageBookInvDelta','ItemDataStorageBookInvFinal','ItemDataStorageBookInvValue','ItemDataActualStorageQty','ItemDataDeltaStorageQty','ItemDataFinalStorageQty','ItemDataDeltaStorageQty','ItemDataFinalStorageQty','ItemDataActualSupplierStorageOrdered','ItemDataDeltaSupplierStorageOrdered','ItemDataFinalSupplierStorageOrdered','ItemDataDeltaSupplierStorageOrdered','ItemDataFinalSupplierStorageOrdered','ItemDataStorageProdInvActual','ItemDataStorageProdInvDelta','ItemDataStorageProdInvFinal','ItemDataStorageProdInvDelta','ItemDataStorageProdInvFinal','ItemDataActCustStorageReserved','ItemDataDeltaCustStorageReserved','ItemDataFinCustStorageReserved','ItemDataDeltaCustStorageReserved','ItemDataFinCustStorageReserved','ItemDataActualStorageAllocated','ItemDataDeltaStorageAllocated','ItemDataFinalStorageAllocated','ItemDataDeltaStorageAllocated','ItemDataFinalStorageAllocated','ItemDataStorageActReservedByProd','ItemDataStorageDeltaReservedByProd','ItemDataStorageFinReservedByProd','ItemDataStorageDeltaReservedByProd','ItemDataStorageFinReservedByProd','ItemDataActualStorageAvailaibility','ItemDataFinalStorageAvailaibility','ItemDataFinalStorageAvailaibility','ItemDataStorageMinimumStock','ItemDataStorageMaximumStock','ItemDataOnHandActual','ItemDataFinalOnHand','ItemDataMinimumStock','AvailabilityAnalysisDetails','LifoFifoHistory','ManItemDataDrawing','ManItemDataInProcessBOM','ManItemDataNotes','ManItemDataMRPPolicy','ManItemDataLeadTime','ManItemDataOrderReleaseDays','ManItemDataRMCost','ManItemDataSetupCost','ManItemDataInHouseProcessingCost','ManItemDataOutsourcedProcessingCost','ManItemDataProductionCost','ManItemDataProductionCostLastChange','ManItemDataItemData_LastCost','ManItemDataMO','ManItemDataStandardCost','DBTManItemDataManufacturing','ManItemDatabTreeView','DBTManItemDataTechnicalData','WMSItemDataStorage','WMSItemDataForPutawayByZone','WMSItemDataForPickingByZone','WMSItemDataBlockedByZone','DBTWMSItemDataZone','WMSItemDataForPutawayByBin','WMSItemDataForPickingByBin','WMSItemDataBlockedByBin','DBTWMSItemDataBin','WMSItemDataForPutawayByStock','WMSItemDataForPickingByStock','WMSItemDataBlockedByStock','DBTWMSItemDataStocks','WMSItemDataCrossDockingTotal','DBTWMSItemDataCrossDocking','InHouseCreatedPic','sInHouseCreatedLbl','InHouseReleasedPic','sInHouseReleasedLbl','InHouseInProcessingPic','sInHouseInProcessingLbl','InHouseConfirmedPic','sInHouseConfirmedLbl','ConfirmTOPic','ReleasedPic','InProcessingPic','ConfirmedPic','RequirementPic','sRequirementLbl','ClosedLinePic','sClosedLineLbl','ReqNoPostablePic','sReqNoPostableLbl','PartialClosePic','sPartialCloseLbl','CloseNoPickPic','sCloseNoPickLbl','DNPic','DNErrorPic','BoLPic','BoLErrorPic','OrderPic','OrderDeletePic','OrderErrorPic','ToolBmp','FamilyBmp'],'MOComponents':['StringLine','BmpProperty','Drawing','Component','Variant','ComponentNature','NotEnter','UoM','NeededQty','FixedQty','Valorize','PickedQuantity','DNRtgStep','TechnicalData','AccountingType','Waste','WasteQuantity','Notes','DNQuantity','ActualCost','WasteUoM','Lot','FromKanban','ParentBOM','ParentVar'],'HKLItemMOCOMP':['Description'],'HKLWasteBE':['Description'],'HKLSupplier':['CompanyName'],'MOSteps':['StateBmp','SetupTime','ScrapQuantity','Supplier','StateBmp','SetupTime','ScrapQuantity','Supplier'],'@DBTLabour':['__ePhase','__bIsWorker','__eResourceType','__sResourceCode','__nWorkerID','__sResourceDescription','__eLabourType','__bIsPercent','__nAttendancePerc','__nWorkingTime','__dDate','__nNoOfResources'],'DBTToolsManagement':['RtgStepTypeBmp','RtgStep','Alternate','AltRtgStep','Operation','OperationDescription','Usage','IsFamily','Tool','ProcessingType','ToolType','Fixed','UsageQuantity','UsageTime','ExclusiveUse','Source','SourceTool'],'HKLSelectorTools':['Description'],'HKLAccountingType':['Description'],'HKLCustomer':['CompNameComplete'],'MOStepsKanban':['Enabled','BOM','Variant','Operation','WC','IsOutsourced','SetupTime','ProcessingTime','OutsourcedProcCost','InHouseProcCost','SetupCost','ParentBOM','ParentVar'],'MOProcessing':['BmpDoc','BOMSubcontracting','DocumentNumber','DocumentDate','Supplier'],'HKLSubcontractor':['CompanyName'],'MOHierarchies':['ParentMO','Item','Variant','Qty','UoM','MODate','JobNo','SaleOrdNo','SaleOrdPos','CustomerOrdDate','Customer','CompanyName'],'HKLItemsByGoodType':['Description'],'MOInventoryEntries':['IEBitmap','ManufacturingEntryType','ManufacturingOutsrcRtgStep','InvRsn','IEDescription','DocNo','PostingDate','StoragePhase1','SpecificatorPhase1','StoragePhase2','SpecificatorPhase2','IECorrection'],'MOLots':['Item','Lot','SupplierLotNo','ValidFrom','Description'],'RequirementsOrigin':['Product','RequiredQuantity','PurchaseReqNo','Line','Job','PurchaseOrdNo','PurchaseOrdPos','OrderedQty','ParentMONo','RequiredQuantity'],'DBTLinksTable':['Image','Description'],'AvailabilityAnalysisDetails':['FromDate','IssuedQuantityBaseUoM','PickedQuantityBaseUoM','ProgrBalanceQty','CodeType','DocumentNumber','Line','CustSupp','CompanyName','Job','UoM','DocQty'],'LifoFifoHistory':['ReceiptBatchId','Storage','AccountingType','InvEntryType','PostingDate','Qty','LineCost'],'DBTManItemDataManufacturing':['DeliveryDate','MONo','UoM','ProductionQty','MOStatus'],'DBTManItemDataTechnicalData':['Name','NumberValue','StringValue','DateValue','BoolValue','Notes'],'DBTWMSItemDataZone':['Zone','ZoneDescri','ConsignmentPartner','AvailableQty','SumQtyBaseUoM','SumQtyReserved','SumQtyIncoming','SumQtyIsMultilevelSU','ForPicking','ForPutaway','Blocked','SpecialStock'],'DBTWMSItemDataBin':['Zone','ZoneDescri','Bin','ConsignmentPartner','AvailableQty','SumQtyBaseUoM','SumQtyReserved','SumQtyIncoming','SumQtyIsMultilevelSU','ForPicking','ForPutaway','Blocked','SpecialStock'],'DBTWMSItemDataStocks':['Zone','ZoneDescri','Bin','Lot','LotValidTo','SpecialStock','ConsignmentPartner','AvailableQty','SumQtyBaseUoM','SumQtyReserved','SumQtyIncoming','SumQtyIsMultilevelSU','UnitOfMeasure','SumQty','ForPicking','ForPutaway','Blocked'],'DBTWMSItemDataCrossDocking':['Lot','ConsignmentPartner','QtyBaseUoM']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_MAN_ORDFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_MAN_ORDComponent, resolver);
    }
} 