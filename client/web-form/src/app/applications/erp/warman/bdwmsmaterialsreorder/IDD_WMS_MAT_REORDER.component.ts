﻿import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_WMS_MAT_REORDERService } from './IDD_WMS_MAT_REORDER.service';

@Component({
    selector: 'tb-IDD_WMS_MAT_REORDER',
    templateUrl: './IDD_WMS_MAT_REORDER.component.html',
    providers: [IDD_WMS_MAT_REORDERService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_WMS_MAT_REORDERComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_WMS_MAT_REORDERService,
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
        
        		this.bo.appendToModelStructure({'global':['HFMO_From','HFMO_To','HFMO_All','HFMO_Range','HFMO_From','bMOStepBoth','bMOStepInHouse','bMOStepOutSourced','HFItems_All','HFItems_Range','HFItems_From','HFItems_To','HFVariant_All','HFVariant_Range','HFVariant_From','HFVariant_To','HFMODeliveryDate_All','HFMODeliveryDate_Range','HFMODeliveryDate_From','HFMODeliveryDate_To','HFMOJob_From','HFMOJob_To','HFMOJob_All','HFMOJob_Range','HFMOJob_From','HFSaleOrder_From','HFSaleOrder_To','HFSaleOrder_All','HFSaleOrder_Range','HFSaleOrder_From','DBTWMSMaterialsReorder','ItemDataBookInventoryActual','ItemDataBookInvDelta','ItemDataFinalBookInv','ItemDataBookInvDelta','ItemDataFinalBookInv','ItemDataBookInvValue','ItemDataOnHandActual','ItemDataDeltaOnHand','ItemDataFinalOnHand','ItemDataDeltaOnHand','ItemDataFinalOnHand','ItemDataOrderedSupplierActual','ItemDataOrderedSupplierDelta','ItemDataOrderedSupplierIsFinal','ItemDataOrderedSupplierDelta','ItemDataOrderedSupplierIsFinal','ItemDataActInProduction','ItemDataDeltaInProduction','ItemDataFinInProduction','ItemDataDeltaInProduction','ItemDataFinInProduction','ItemDataActCustReserved','ItemDataDeltaCustReserved','ItemDataFinCustReserved','ItemDataDeltaCustReserved','ItemDataFinCustReserved','ItemDataActualAllocated','ItemDataDeltaAllocated','ItemDataFinalAllocated','ItemDataDeltaAllocated','ItemDataFinalAllocated','ItemDataActReservedByProd','ItemDataDeltaReservedByProd','ItemDataFinReservedByProd','ItemDataDeltaReservedByProd','ItemDataFinReservedByProd','ItemDataActualAvailaibility','ItemDataFinalAvailaibility','ItemDataFinalAvailaibility','ItemDataKind','ItemDataValueWith','ItemDataNetWeight','ItemDataGrossWeight','ItemDataMinimumStock','ItemDataMaxStock','ItemDataReorderingLot','ItemDataProductionLot','ItemDataItemPicture','ItemDataStorage','ItemDataStorageBookInventoryActual','ItemDataStorageBookInvDelta','ItemDataStorageBookInvFinal','ItemDataStorageBookInvDelta','ItemDataStorageBookInvFinal','ItemDataStorageBookInvValue','ItemDataActualStorageQty','ItemDataDeltaStorageQty','ItemDataFinalStorageQty','ItemDataDeltaStorageQty','ItemDataFinalStorageQty','ItemDataActualSupplierStorageOrdered','ItemDataDeltaSupplierStorageOrdered','ItemDataFinalSupplierStorageOrdered','ItemDataDeltaSupplierStorageOrdered','ItemDataFinalSupplierStorageOrdered','ItemDataStorageProdInvActual','ItemDataStorageProdInvDelta','ItemDataStorageProdInvFinal','ItemDataStorageProdInvDelta','ItemDataStorageProdInvFinal','ItemDataActCustStorageReserved','ItemDataDeltaCustStorageReserved','ItemDataFinCustStorageReserved','ItemDataDeltaCustStorageReserved','ItemDataFinCustStorageReserved','ItemDataActualStorageAllocated','ItemDataDeltaStorageAllocated','ItemDataFinalStorageAllocated','ItemDataDeltaStorageAllocated','ItemDataFinalStorageAllocated','ItemDataStorageActReservedByProd','ItemDataStorageDeltaReservedByProd','ItemDataStorageFinReservedByProd','ItemDataStorageDeltaReservedByProd','ItemDataStorageFinReservedByProd','ItemDataActualStorageAvailaibility','ItemDataFinalStorageAvailaibility','ItemDataFinalStorageAvailaibility','ItemDataStorageMinimumStock','ItemDataStorageMaximumStock','ItemDataOnHandActual','ItemDataFinalOnHand','ItemDataMinimumStock','AvailabilityAnalysisDetails','LifoFifoHistory','ManItemDataDrawing','ManItemDataInProcessBOM','ManItemDataNotes','ManItemDataMRPPolicy','ManItemDataLeadTime','ManItemDataOrderReleaseDays','ManItemDataRMCost','ManItemDataSetupCost','ManItemDataInHouseProcessingCost','ManItemDataOutsourcedProcessingCost','ManItemDataProductionCost','ManItemDataProductionCostLastChange','ManItemDataItemData_LastCost','ManItemDataMO','ManItemDataStandardCost','DBTManItemDataManufacturing','ManItemDatabTreeView','DBTManItemDataTechnicalData','WMSItemDataStorage','WMSItemDataForPutawayByZone','WMSItemDataForPickingByZone','WMSItemDataBlockedByZone','DBTWMSItemDataZone','WMSItemDataForPutawayByBin','WMSItemDataForPickingByBin','WMSItemDataBlockedByBin','DBTWMSItemDataBin','WMSItemDataForPutawayByStock','WMSItemDataForPickingByStock','WMSItemDataBlockedByStock','DBTWMSItemDataStocks','WMSItemDataCrossDockingTotal','DBTWMSItemDataCrossDocking'],'DBTWMSMaterialsReorder':['Selected','Item','Storage','UoM','RequiredQty','ConfirmedTOQty','AvailStorages','AvailableQty','ReleasedQty'],'HKLItemsBE':['Description'],'HKLStoragesBE':['Description'],'AvailabilityAnalysisDetails':['FromDate','IssuedQuantityBaseUoM','PickedQuantityBaseUoM','ProgrBalanceQty','CodeType','DocumentNumber','Line','CustSupp','CompanyName','Job','UoM','DocQty'],'LifoFifoHistory':['ReceiptBatchId','Storage','AccountingType','InvEntryType','PostingDate','Qty','LineCost'],'DBTManItemDataManufacturing':['DeliveryDate','MONo','UoM','ProductionQty','MOStatus'],'DBTManItemDataTechnicalData':['Name','NumberValue','StringValue','DateValue','BoolValue','Notes'],'DBTWMSItemDataZone':['Zone','ZoneDescri','ConsignmentPartner','AvailableQty','SumQtyBaseUoM','SumQtyReserved','SumQtyIncoming','SumQtyIsMultilevelSU','ForPicking','ForPutaway','Blocked','SpecialStock'],'DBTWMSItemDataBin':['Zone','ZoneDescri','Bin','ConsignmentPartner','AvailableQty','SumQtyBaseUoM','SumQtyReserved','SumQtyIncoming','SumQtyIsMultilevelSU','ForPicking','ForPutaway','Blocked','SpecialStock'],'DBTWMSItemDataStocks':['Zone','ZoneDescri','Bin','Lot','LotValidTo','SpecialStock','ConsignmentPartner','AvailableQty','SumQtyBaseUoM','SumQtyReserved','SumQtyIncoming','SumQtyIsMultilevelSU','UnitOfMeasure','SumQty','ForPicking','ForPutaway','Blocked'],'DBTWMSItemDataCrossDocking':['Lot','ConsignmentPartner','QtyBaseUoM']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_WMS_MAT_REORDERFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_WMS_MAT_REORDERComponent, resolver);
    }
} 