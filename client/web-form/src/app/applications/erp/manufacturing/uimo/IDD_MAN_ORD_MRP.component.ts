import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_MAN_ORD_MRPService } from './IDD_MAN_ORD_MRP.service';

@Component({
    selector: 'tb-IDD_MAN_ORD_MRP',
    templateUrl: './IDD_MAN_ORD_MRP.component.html',
    providers: [IDD_MAN_ORD_MRPService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_MAN_ORD_MRPComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_MAN_ORD_A_MIS_itemSource: any;
public IDC_ORD_RTGSTEPS_MOCOMP_UOM_itemSource: any;
public IDC_ORD_RTGSTEPS_MOCOMP_RTGSTEP_SENT_itemSource: any;

    constructor(document: IDD_MAN_ORD_MRPService,
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

        const boService = this.document as BOService;
		boService.appendToModelStructure({'MO':['MONo','ProductionQty','UoM','DeliveryDate','BOM','Variant','ProductionQty','UoM','DeliveryDate','UoM','DeliveryDate','MRPConfirmationRank','Simulation','GroupSF','TMO_ConfirmationLevelDescr','ProductionPlanLine','Job','ProductionLotNumber','CostCenter','Drawing','AccountingType','ProducedQty','ScrapQuantity','SecondRateQuantity','Feasibility','BarcodeSegment','TMO_PickingListNo','Feasibility','BarcodeSegment','CreationDate','LastModificationDate','RunDate','CostsCalculationLastDate','MRPConfirmationRank','Simulation','GroupSF','TMO_ConfirmationLevelDescr','ProductionPlanLine','Printed','PrintDate','Notes','Customer','InternalOrdNo','Position','ECODate','ECORevision','ProcessingBudgetCost','ProcessingActualCost','ProcessingBudgetCost','SetupBudgetCost','LabourBudgetCost','EstimatedMaterialCostkanban','EstimatedProcCostkanban','ProcessingActualCost','SetupActualCost','LabourActualCost','ActualMaterialCostkanban','ActualProcCostkanban','SimStartDate','SimEndDate','EstimatedProcessingTime','EstimatedSetupTime','StartingDate','EndingDate','ActualProcessingTime','ActualSetupTime'],'HKLItem':['Description'],'global':['nMOStatusMax','MOStatusDescription','ImageStatusMO','MOProdDescription','ProdImage','LeftProducedDescription','RightProducedDescription','MOSRDescription','SRImage','LeftSecondRateDescription','RightSecondRateDescription','MOScrapDescription','ScrapImage','LeftScrapDescription','RightScrapDescription','MOComponents','Internal','Outsourced','Supplier','MOSteps','MOSteps','NrSchema','NrSchema','SaleOrdQty','SaleOrdUoM','CustOrdDate','ExpectedMaterialCost','CostTotExpected','ActualMaterialCost','CostTotDeliv','ExpectedMaterialCost','CostTotExpected','CostTotExpectedKanban','ActualMaterialCost','CostTotDeliv','CostTotDelivKanban','MOProcessing','MOHierarchies','MOInventoryEntries','MOLots','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'MOComponents':['StringLine','BmpProperty','Drawing','Component','Variant','ComponentNature','NotEnter','UoM','NeededQty','FixedQty','Valorize','PickedQuantity','DNRtgStep','TechnicalData','AccountingType','Waste','WasteQuantity','Notes','DNQuantity','ActualCost','WasteUoM','Lot','FromKanban','ParentBOM','ParentVar'],'HKLItemMOCOMP':['Description'],'HKLWasteBE':['Description'],'HKLSupplier':['CompanyName'],'MOSteps':['StateBmp','SetupTime','ScrapQuantity','Supplier','StateBmp','SetupTime','ScrapQuantity','Supplier'],'HKLAccountingType':['Description'],'HKLCustomer':['CompNameComplete'],'MOProcessing':['BmpDoc','BOMSubcontracting','DocumentNumber','DocumentDate','Supplier'],'HKLSubcontractor':['CompanyName'],'MOHierarchies':['ParentMO','Item','Variant','Qty','UoM','MODate','JobNo','SaleOrdNo','SaleOrdPos','CustomerOrdDate','Customer','CompanyName'],'HKLItemsByGoodType':['Description'],'MOInventoryEntries':['IEBitmap','ManufacturingEntryType','ManufacturingOutsrcRtgStep','InvRsn','IEDescription','DocNo','PostingDate','StoragePhase1','SpecificatorPhase1','StoragePhase2','SpecificatorPhase2','IECorrection'],'MOLots':['Item','Lot','SupplierLotNo','ValidFrom','Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_MAN_ORD_MRPFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_MAN_ORD_MRPComponent, resolver);
    }
} 