﻿import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_PURCHASE_ORDERService } from './IDD_PURCHASE_ORDER.service';

@Component({
    selector: 'tb-IDD_PURCHASE_ORDER',
    templateUrl: './IDD_PURCHASE_ORDER.component.html',
    providers: [IDD_PURCHASE_ORDERService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_PURCHASE_ORDERComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_PURCHASE_ORDERService,
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
        
        const boService = this.document as BOService;
		boService.appendToModelStructure({'PurchaseOrder':['InternalOrdNo','OrderDate','ExternalOrdNo','Supplier','Payment','ExpectedDeliveryDate','ConfirmedDeliveryDate','Payment','NetOfTax','Currency','FixingDate','Fixing','ExpectedDeliveryDate','ConfirmedDeliveryDate','NonStandardPayment','UseBusinessYear','AccTpl','TaxJournal','AccGroup','ContractCode','ProjectCode','TaxCommunicationGroup','Job','CostCenter','ProductLine','SupplierBank','CompanyBank','SupplierCA','CompanyCA','SendDocumentsTo','PaymentAddress','InvRsn','StubBook','StoragePhase1','SpecificatorPhase1','StoragePhase2','SpecificatorPhase2','Cancelled','Delivered','Paid','Receipt','OurReference','YourReference','Notes','Language','BarcodeSegment'],'HKLSupplier':['CompNameCompleteWithTaxNumber'],'global':['DeliveredGauge','DeliveredGaugeDescri','ImageDeliveryStatus','InvoicedGauge','InvoicedGaugeDescri','ImageInvoiceStatus','Detail','TaxSummary','PymtSchedule','InstallmentsTot','Delta','IntallmentRegenerate','AmountRegenerate','DocShipToDescri','BranchPayToDescri','ShipToDescri','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg','ItemDataBookInventoryActual','ItemDataBookInvDelta','ItemDataFinalBookInv','ItemDataBookInvDelta','ItemDataFinalBookInv','ItemDataBookInvValue','ItemDataOnHandActual','ItemDataDeltaOnHand','ItemDataFinalOnHand','ItemDataDeltaOnHand','ItemDataFinalOnHand','ItemDataOrderedSupplierActual','ItemDataOrderedSupplierDelta','ItemDataOrderedSupplierIsFinal','ItemDataOrderedSupplierDelta','ItemDataOrderedSupplierIsFinal','ItemDataActInProduction','ItemDataDeltaInProduction','ItemDataFinInProduction','ItemDataDeltaInProduction','ItemDataFinInProduction','ItemDataActCustReserved','ItemDataDeltaCustReserved','ItemDataFinCustReserved','ItemDataDeltaCustReserved','ItemDataFinCustReserved','ItemDataActualAllocated','ItemDataDeltaAllocated','ItemDataFinalAllocated','ItemDataDeltaAllocated','ItemDataFinalAllocated','ItemDataActReservedByProd','ItemDataDeltaReservedByProd','ItemDataFinReservedByProd','ItemDataDeltaReservedByProd','ItemDataFinReservedByProd','ItemDataActualAvailaibility','ItemDataFinalAvailaibility','ItemDataFinalAvailaibility','ItemDataKind','ItemDataValueWith','ItemDataNetWeight','ItemDataGrossWeight','ItemDataMinimumStock','ItemDataMaxStock','ItemDataReorderingLot','ItemDataProductionLot','ItemDataItemPicture','ItemDataStorage','ItemDataStorageBookInventoryActual','ItemDataStorageBookInvDelta','ItemDataStorageBookInvFinal','ItemDataStorageBookInvDelta','ItemDataStorageBookInvFinal','ItemDataStorageBookInvValue','ItemDataActualStorageQty','ItemDataDeltaStorageQty','ItemDataFinalStorageQty','ItemDataDeltaStorageQty','ItemDataFinalStorageQty','ItemDataActualSupplierStorageOrdered','ItemDataDeltaSupplierStorageOrdered','ItemDataFinalSupplierStorageOrdered','ItemDataDeltaSupplierStorageOrdered','ItemDataFinalSupplierStorageOrdered','ItemDataStorageProdInvActual','ItemDataStorageProdInvDelta','ItemDataStorageProdInvFinal','ItemDataStorageProdInvDelta','ItemDataStorageProdInvFinal','ItemDataActCustStorageReserved','ItemDataDeltaCustStorageReserved','ItemDataFinCustStorageReserved','ItemDataDeltaCustStorageReserved','ItemDataFinCustStorageReserved','ItemDataActualStorageAllocated','ItemDataDeltaStorageAllocated','ItemDataFinalStorageAllocated','ItemDataDeltaStorageAllocated','ItemDataFinalStorageAllocated','ItemDataStorageActReservedByProd','ItemDataStorageDeltaReservedByProd','ItemDataStorageFinReservedByProd','ItemDataStorageDeltaReservedByProd','ItemDataStorageFinReservedByProd','ItemDataActualStorageAvailaibility','ItemDataFinalStorageAvailaibility','ItemDataFinalStorageAvailaibility','ItemDataStorageMinimumStock','ItemDataStorageMaximumStock','ItemDataOnHandActual','ItemDataFinalOnHand','ItemDataMinimumStock','AvailabilityAnalysisDetails','LifoFifoHistory','ManItemDataDrawing','ManItemDataInProcessBOM','ManItemDataNotes','ManItemDataMRPPolicy','ManItemDataLeadTime','ManItemDataOrderReleaseDays','ManItemDataRMCost','ManItemDataSetupCost','ManItemDataInHouseProcessingCost','ManItemDataOutsourcedProcessingCost','ManItemDataProductionCost','ManItemDataProductionCostLastChange','ManItemDataItemData_LastCost','ManItemDataMO','ManItemDataStandardCost','DBTManItemDataManufacturing','ManItemDatabTreeView','DBTManItemDataTechnicalData','WMSItemDataStorage','WMSItemDataForPutawayByZone','WMSItemDataForPickingByZone','WMSItemDataBlockedByZone','DBTWMSItemDataZone','WMSItemDataForPutawayByBin','WMSItemDataForPickingByBin','WMSItemDataBlockedByBin','DBTWMSItemDataBin','WMSItemDataForPutawayByStock','WMSItemDataForPickingByStock','WMSItemDataBlockedByStock','DBTWMSItemDataStocks','WMSItemDataCrossDockingTotal','DBTWMSItemDataCrossDocking','RequirementsOrigin','DBTLinksTable'],'HKLPaymentTerms':['Description','Description'],'HKLCurrenciesCurrObj':['Description'],'Detail':['PurchaseReqNo','PurchaseReqPos','ConfirmationNum','ConfirmedDeliveryDate','PreviousConfirmedDeliveryDate','ExtendedNotes'],'Shipping':['NoOfPacks','NetWeight','GrossVolume','GrossWeight','Carrier1','Carrier2','Carrier3','Port','Transport','ShipToCustSuppType','ShipToCustSupp','ShipToAddress','Package','Shipping'],'Charges':['FreeSamplesDocCurr','DiscountFormula','Discounts','TotalAmount','PayableAmount','ShippingCharges','InsuranceCharges','AdditionalCharges','ShippingCharges','ShippingChargesTaxCode','PackagingCharges','CollectionCharges','CollectionChargesTaxCode','AdditionalCharges','StampsCharges','StampsChargesTaxCode','GoodsAmount','ServiceAmounts','FreeSamplesDocCurr','FreeSamples','DiscountOnGoods','Discounts','DiscountOnServices','DiscountFormula','GeneralDiscountTot','TotalAmount','TotalAmountDocCurr','AdvanceAmount','Allowances','PayableAmount','PayableAmountInBaseCurr','PayableAmount','CashOnDeliveryPercentage','CashOnDeliveryCharges'],'TaxSummary':['TaxCode','TaxableAmount','TaxAmount','TotalAmount','TaxableAmountDocCurr','TaxAmountDocCurr','TotalAmountDocCurr'],'PymtSchedule':['InstallmentNo','InstallmentType','InstallmStartDate','DueDateDays','InstallmentTot','Amount','TaxAmount'],'HKLAccTpl':['Description'],'HKLJournal':['Description'],'HKLAccGroup':['Description'],'HKLContractCodes':['Description'],'HKLProjectCodes':['Description'],'HKLTaxCommunicationGroup':['Description'],'HKLJobs':['Description'],'HKLCstCenter':['Description'],'HKLProductLine':['Description'],'HKLSupplierBank':['Description'],'HKLCompanyBank':['Description'],'HKLInvEntr':['Description'],'HKLStubBook':['Description'],'HKLStorageF1':['Description'],'HKLSpecificatorF1':['CompanyName'],'HKLStorageF2':['Description'],'HKLSpecificatorF2':['CompanyName'],'HKLCarrier_1':['CompNameComplete'],'HKLCarrier_2':['CompNameComplete'],'HKLCarrier_3':['CompNameComplete'],'HKLPorts':['Description'],'HKLTransportMode':['Description'],'HKLPackages':['Description'],'HKLShippingBy':['Description'],'HKLLanguages':['Description'],'Notes':['Notes'],'AvailabilityAnalysisDetails':['FromDate','IssuedQuantityBaseUoM','PickedQuantityBaseUoM','ProgrBalanceQty','CodeType','DocumentNumber','Line','CustSupp','CompanyName','Job','UoM','DocQty'],'LifoFifoHistory':['ReceiptBatchId','Storage','AccountingType','InvEntryType','PostingDate','Qty','LineCost'],'DBTManItemDataManufacturing':['DeliveryDate','MONo','UoM','ProductionQty','MOStatus'],'DBTManItemDataTechnicalData':['Name','NumberValue','StringValue','DateValue','BoolValue','Notes'],'DBTWMSItemDataZone':['Zone','ZoneDescri','ConsignmentPartner','AvailableQty','SumQtyBaseUoM','SumQtyReserved','SumQtyIncoming','SumQtyIsMultilevelSU','ForPicking','ForPutaway','Blocked','SpecialStock'],'DBTWMSItemDataBin':['Zone','ZoneDescri','Bin','ConsignmentPartner','AvailableQty','SumQtyBaseUoM','SumQtyReserved','SumQtyIncoming','SumQtyIsMultilevelSU','ForPicking','ForPutaway','Blocked','SpecialStock'],'DBTWMSItemDataStocks':['Zone','ZoneDescri','Bin','Lot','LotValidTo','SpecialStock','ConsignmentPartner','AvailableQty','SumQtyBaseUoM','SumQtyReserved','SumQtyIncoming','SumQtyIsMultilevelSU','UnitOfMeasure','SumQty','ForPicking','ForPutaway','Blocked'],'DBTWMSItemDataCrossDocking':['Lot','ConsignmentPartner','QtyBaseUoM'],'RequirementsOrigin':['Product','RequiredQuantity','PurchaseReqNo','Line','Job','PurchaseOrdNo','PurchaseOrdPos','OrderedQty','ParentMONo','RequiredQuantity'],'DBTLinksTable':['Image','Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_PURCHASE_ORDERFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_PURCHASE_ORDERComponent, resolver);
    }
} 