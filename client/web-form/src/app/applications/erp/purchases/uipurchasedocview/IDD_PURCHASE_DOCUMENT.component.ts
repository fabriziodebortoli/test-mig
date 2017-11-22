import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_PURCHASE_DOCUMENTService } from './IDD_PURCHASE_DOCUMENT.service';

@Component({
    selector: 'tb-IDD_PURCHASE_DOCUMENT',
    templateUrl: './IDD_PURCHASE_DOCUMENT.component.html',
    providers: [IDD_PURCHASE_DOCUMENTService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_PURCHASE_DOCUMENTComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_PURCH_DOC_NATURE_TRANSACTION_itemSource: any;
public IDC_PURCH_DOC_CODETYPE_SPECIFICATOR_1_itemSource: any;
public IDC_PURCH_DOC_CODETYPE_SPECIFICATOR_2_itemSource: any;
public IDC_PURCH_DOC_SPEC_PHASE_1_TORETURN_itemSource: any;
public IDC_PURCH_DOC_SPEC_PHASE_2_TORETURN_itemSource: any;
public IDC_PURCH_DOC_SPEC_PHASE_1_DISCARDS_itemSource: any;
public IDC_PURCH_DOC_SPEC_PHASE_2_DISCARDS_itemSource: any;
public IDC_PURCH_DOC_SPEC_PHASE_1_ORDCOL_itemSource: any;
public IDC_PURCH_DOC_SPEC_PHASE_2_ORDCOL_itemSource: any;

    constructor(document: IDD_PURCHASE_DOCUMENTService,
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
        this.IDC_PURCH_DOC_NATURE_TRANSACTION_itemSource = {
  "name": "NatureDispatchesItemSource",
  "namespace": "ERP.Intrastat.Documents.NatureDispatchesItemSource"
}; 
this.IDC_PURCH_DOC_CODETYPE_SPECIFICATOR_1_itemSource = {
  "name": "ConformingSpecificator1TypeCombo",
  "namespace": "ERP.Inventory.Components.SpecTypeNoIgnoreCombo"
}; 
this.IDC_PURCH_DOC_CODETYPE_SPECIFICATOR_2_itemSource = {
  "name": "ConformingSpecificator2TypeCombo",
  "namespace": "ERP.Inventory.Components.SpecTypeNoIgnoreCombo"
}; 
this.IDC_PURCH_DOC_SPEC_PHASE_1_TORETURN_itemSource = {
  "name": "ReturnSpecificator1TypeCombo",
  "namespace": "ERP.Inventory.Components.SpecTypeNoIgnoreCombo"
}; 
this.IDC_PURCH_DOC_SPEC_PHASE_2_TORETURN_itemSource = {
  "name": "ReturnSpecificator2TypeCombo",
  "namespace": "ERP.Inventory.Components.SpecTypeNoIgnoreCombo"
}; 
this.IDC_PURCH_DOC_SPEC_PHASE_1_DISCARDS_itemSource = {
  "name": "ScrapSpecificator1TypeCombo",
  "namespace": "ERP.Inventory.Components.SpecTypeNoIgnoreCombo"
}; 
this.IDC_PURCH_DOC_SPEC_PHASE_2_DISCARDS_itemSource = {
  "name": "ScrapSpecificator2TypeCombo",
  "namespace": "ERP.Inventory.Components.SpecTypeNoIgnoreCombo"
}; 
this.IDC_PURCH_DOC_SPEC_PHASE_1_ORDCOL_itemSource = {
  "name": "InspectionSpecificator1TypeCombo",
  "namespace": "ERP.Inventory.Components.SpecTypeNoIgnoreCombo"
}; 
this.IDC_PURCH_DOC_SPEC_PHASE_2_ORDCOL_itemSource = {
  "name": "InspectionSpecificator2TypeCombo",
  "namespace": "ERP.Inventory.Components.SpecTypeNoIgnoreCombo"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'PurchaseDocument':['DocNo','SupplierDocNo','DocumentDate','SupplierDocDate','PostingDate','Supplier','ConfInvRsn','AccrualDate','TaxAccrualDate','TaxAccrualDate','ModifyOriginalPymtSched','Payment','NetOfTax','InvoiceFollows','EOYDeductible','Currency','FixingDate','Fixing','ConfInvRsn','Payment','TaxAccrualDate','SaleRecordNo','NetOfTax','InvoiceFollows','EOYDeductible','ModifyOriginalPymtSched','Currency','FixingDate','Fixing','InstallmStartDate','AccTpl','TaxJournal','EUTaxJournal','SaleRecordNo','SaleRecordNo','InvoicingAccGroup','InvoicingAccTpl','InvoicingTaxJournal','InvoicingAccGroup','ContractCode','ProjectCode','TaxCommunicationGroup','TaxCommunicationOperation','PlafondAccrualDate','PureJECollectionPaymentNo','Job','CostCenter','ProductLine','SupplierBank','CompanyBank','CompanyCA','SupplierCA','ESRReferenceNumber','ESRCheckDigit','PaymentAddress','IntrastatAccrualDate','IntrastatBis','IntrastatTer','Operation','NatureOfTransaction','DeliveryTerms','ModeOfTransport','CountryOfConsignment','CountryOfPayment','CountryOfOrigin','CountryOfTransport','ConfInvRsn','StubBook','ConformingStorage1','ConformingSpecificator1Type','ConformingSpecificator1','ConformingSpecificator1','ConformingStorage2','ConformingSpecificator2Type','ConformingSpecificator2','ConformingSpecificator2','ActionOnLifoFifo','RMAStubBook','ReturnInvRsn','ReturnStorage1','ReturnSpecificator1Type','ReturnSpecificator1','ReturnStorage2','ReturnSpecificator2Type','ReturnSpecificator2','ScrapInvRsn','ScrapStorage1','ScrapSpecificator1Type','ScrapSpecificator1','ScrapStorage2','ScrapSpecificator2Type','ScrapSpecificator2','InspectionLoadInvRsn','InspectionStorage1','InspectionSpecificator1Type','InspectionSpecificator1','InspectionStorage2','InspectionSpecificator2Type','InspectionSpecificator2','Invoiced','OurReference','YourReference','Language','Notes'],'HKLSupplier':['CompNameCompleteWithTaxNumber'],'global':['ReversedGauge','ReversedGaugeDescri','ImageReversedStatus','InvoicedGauge','InvoicedGaugeDescri','ImageInvoicedStatus','bIsAProtocol','Detail','TaxSummary','PymtSchedule','PurchPymtInstallmentsTot','PurchPymtDelta','PurchPymtIntallmentRegenerate','PurchPymtAmountRegenerate','PymtScheduleSearch','PymtScheduleDescri','AdvanceInstallmentSearch','AdvanceInstallmentDescri','AdvanceInstallmentDescri','SendPaymentToDescri','PurchPymtShipToDescri','Notes','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg','ItemDataBookInventoryActual','ItemDataBookInvDelta','ItemDataFinalBookInv','ItemDataBookInvDelta','ItemDataFinalBookInv','ItemDataBookInvValue','ItemDataOnHandActual','ItemDataDeltaOnHand','ItemDataFinalOnHand','ItemDataDeltaOnHand','ItemDataFinalOnHand','ItemDataOrderedSupplierActual','ItemDataOrderedSupplierDelta','ItemDataOrderedSupplierIsFinal','ItemDataOrderedSupplierDelta','ItemDataOrderedSupplierIsFinal','ItemDataActInProduction','ItemDataDeltaInProduction','ItemDataFinInProduction','ItemDataDeltaInProduction','ItemDataFinInProduction','ItemDataActCustReserved','ItemDataDeltaCustReserved','ItemDataFinCustReserved','ItemDataDeltaCustReserved','ItemDataFinCustReserved','ItemDataActualAllocated','ItemDataDeltaAllocated','ItemDataFinalAllocated','ItemDataDeltaAllocated','ItemDataFinalAllocated','ItemDataActReservedByProd','ItemDataDeltaReservedByProd','ItemDataFinReservedByProd','ItemDataDeltaReservedByProd','ItemDataFinReservedByProd','ItemDataActualAvailaibility','ItemDataFinalAvailaibility','ItemDataFinalAvailaibility','ItemDataKind','ItemDataValueWith','ItemDataNetWeight','ItemDataGrossWeight','ItemDataMinimumStock','ItemDataMaxStock','ItemDataReorderingLot','ItemDataProductionLot','ItemDataItemPicture','ItemDataStorage','ItemDataStorageBookInventoryActual','ItemDataStorageBookInvDelta','ItemDataStorageBookInvFinal','ItemDataStorageBookInvDelta','ItemDataStorageBookInvFinal','ItemDataStorageBookInvValue','ItemDataActualStorageQty','ItemDataDeltaStorageQty','ItemDataFinalStorageQty','ItemDataDeltaStorageQty','ItemDataFinalStorageQty','ItemDataActualSupplierStorageOrdered','ItemDataDeltaSupplierStorageOrdered','ItemDataFinalSupplierStorageOrdered','ItemDataDeltaSupplierStorageOrdered','ItemDataFinalSupplierStorageOrdered','ItemDataStorageProdInvActual','ItemDataStorageProdInvDelta','ItemDataStorageProdInvFinal','ItemDataStorageProdInvDelta','ItemDataStorageProdInvFinal','ItemDataActCustStorageReserved','ItemDataDeltaCustStorageReserved','ItemDataFinCustStorageReserved','ItemDataDeltaCustStorageReserved','ItemDataFinCustStorageReserved','ItemDataActualStorageAllocated','ItemDataDeltaStorageAllocated','ItemDataFinalStorageAllocated','ItemDataDeltaStorageAllocated','ItemDataFinalStorageAllocated','ItemDataStorageActReservedByProd','ItemDataStorageDeltaReservedByProd','ItemDataStorageFinReservedByProd','ItemDataStorageDeltaReservedByProd','ItemDataStorageFinReservedByProd','ItemDataActualStorageAvailaibility','ItemDataFinalStorageAvailaibility','ItemDataFinalStorageAvailaibility','ItemDataStorageMinimumStock','ItemDataStorageMaximumStock','ItemDataOnHandActual','ItemDataFinalOnHand','ItemDataMinimumStock','AvailabilityAnalysisDetails','LifoFifoHistory','ManItemDataDrawing','ManItemDataInProcessBOM','ManItemDataNotes','ManItemDataMRPPolicy','ManItemDataLeadTime','ManItemDataOrderReleaseDays','ManItemDataRMCost','ManItemDataSetupCost','ManItemDataInHouseProcessingCost','ManItemDataOutsourcedProcessingCost','ManItemDataProductionCost','ManItemDataProductionCostLastChange','ManItemDataItemData_LastCost','ManItemDataMO','ManItemDataStandardCost','DBTManItemDataManufacturing','ManItemDatabTreeView','DBTManItemDataTechnicalData','WMSItemDataStorage','WMSItemDataForPutawayByZone','WMSItemDataForPickingByZone','WMSItemDataBlockedByZone','DBTWMSItemDataZone','WMSItemDataForPutawayByBin','WMSItemDataForPickingByBin','WMSItemDataBlockedByBin','DBTWMSItemDataBin','WMSItemDataForPutawayByStock','WMSItemDataForPickingByStock','WMSItemDataBlockedByStock','DBTWMSItemDataStocks','WMSItemDataCrossDockingTotal','DBTWMSItemDataCrossDocking','DBTLinksTable'],'HKLInvEntr':['Description','Description','Description'],'Shipping':['DepartureDate','DepartureHr','DepartureMn','DepartureMn','DepartureDate','DepartureHr','DepartureMn','DepartureMn','NoOfPacks','NetWeight','GrossVolume','GrossWeight','Carrier1','Carrier2','Carrier3','Port','Transport','ShipToCustSuppType','ShipToCustSupp','ShipTo','Package','PackageDescription','Appearance','Shipping'],'HKLPaymentTerms':['Description','Description'],'HKLCurrenciesCurrObj':['Description','Description'],'Detail':['l_ModifiableLineBmp','SupplierLotNo','SupplierLotExpiryDate'],'Charges':['FreeSamplesDocCurr','DiscountFormula','Discounts','TotalAmount','PayableAmount','ShippingCharges','ShippingChargesTaxCode','ShippingChargesTaxCode','PackagingCharges','CollectionCharges','CollectionChargesTaxCode','CollectionChargesTaxCode','AdditionalCharges','StampsCharges','StampsChargesTaxCode','StampsChargesTaxCode','StatisticalCharges','StatisticalChargesCalc','TaxSummaryIsManual','GoodsAmount','ServiceAmounts','FreeSamplesDocCurr','DiscountOnGoods','Discounts','DiscountOnServices','DiscountFormula','GeneralDiscountTot','TotalAmount','TotalAmountDocCurr','Advance','Allowances','PayableAmount','PayableAmountInBaseCurr','PayableAmount','CreditNotePreviousPeriod','ReturnedMaterial','CashOnDeliveryPercentage','CashOnDeliveryCharges','PostAdvancesToAcc','PaymentTerm','Advance','AdvanceOffset','AdvanceOffset','PrePayedAdvance','PrePayedAdvance'],'HKLTAXSummaryShipping':['Description','Description'],'HKLTAXSummaryCollection':['Description','Description'],'HKLTAXSummaryStamps':['Description','Description'],'TaxSummary':['TaxCode','TaxableAmount','TaxAmount','UndeductibleAmount','TotalAmount','NotInReverseCharge','TaxableAmountDocCurr','TaxAmountDocCurr','UndeductibleAmountDocCurr','TotalAmountDocCurr'],'PymtSchedule':['InstallmentNo','InstallmentType','DueDate','DayName','InstallmentTot','InstallmentAmount','InstallmentTaxAmount','PymtCash','PymtAccount','CostCenter'],'HKLAccTpl':['Description'],'HKLJournal':['Description'],'HKLJournalIntrastatTax':['Description','Description'],'HKLAccGroupDocSumm':['Description','Description'],'HKLAccTplDocSumm':['Description'],'HKLJournalDocSumm':['Description'],'HKLContractCodes':['Description'],'HKLProjectCodes':['Description'],'HKLTaxCommunicationGroup':['Description'],'HKLAdvanceOffset':['Description','Description'],'HKLJobs':['Description'],'HKLCostCenters':['Description'],'HKLProductLine':['Description'],'HKLSupplierBank':['Description'],'HKLCompanyBank':['Description'],'HKLISOCountryCodes':['Description'],'HKLISOCountryCodesPayment':['Description'],'HKLISOCountryCodesOrig':['Description'],'HKLISOCountryCodesTransport':['Description'],'HKLStubBook':['Description'],'HKLStorageF1':['Description'],'HKLSpecificatorF1':['CompanyName','CompanyName'],'HKLStorageF2':['Description'],'HKLSpecificatorF2':['CompanyName','CompanyName'],'HKLCarrier_1':['CompNameComplete'],'HKLCarrier_2':['CompNameComplete'],'HKLCarrier_3':['CompNameComplete'],'HKLRMAStubBook':['Description'],'HKLSpecificatorF1ToReturn':['CompanyName'],'HKLSpecificatorF2ToReturn':['CompanyName'],'HKLSpecificatorF1Discards':['CompanyName'],'HKLSpecificatorF2Discards':['CompanyName'],'HKLSpecificatorF1InspOrd':['CompanyName'],'HKLSpecificatorF2InspOrd':['CompanyName'],'HKLPorts':['Description'],'HKLTransport':['Description'],'HKLGoodsAppearance':['Description'],'HKLShippingBy':['Description'],'HKLLanguages':['Description'],'Notes':['Line','Notes','ChangeDate','GenByConsistencyCheck'],'AvailabilityAnalysisDetails':['FromDate','IssuedQuantityBaseUoM','PickedQuantityBaseUoM','ProgrBalanceQty','CodeType','DocumentNumber','Line','CustSupp','CompanyName','Job','UoM','DocQty'],'LifoFifoHistory':['ReceiptBatchId','Storage','AccountingType','InvEntryType','PostingDate','Qty','LineCost'],'DBTManItemDataManufacturing':['DeliveryDate','MONo','UoM','ProductionQty','MOStatus'],'DBTManItemDataTechnicalData':['Name','NumberValue','StringValue','DateValue','BoolValue','Notes'],'DBTWMSItemDataZone':['Zone','ZoneDescri','ConsignmentPartner','AvailableQty','SumQtyBaseUoM','SumQtyReserved','SumQtyIncoming','SumQtyIsMultilevelSU','ForPicking','ForPutaway','Blocked','SpecialStock'],'DBTWMSItemDataBin':['Zone','ZoneDescri','Bin','ConsignmentPartner','AvailableQty','SumQtyBaseUoM','SumQtyReserved','SumQtyIncoming','SumQtyIsMultilevelSU','ForPicking','ForPutaway','Blocked','SpecialStock'],'DBTWMSItemDataStocks':['Zone','ZoneDescri','Bin','Lot','LotValidTo','SpecialStock','ConsignmentPartner','AvailableQty','SumQtyBaseUoM','SumQtyReserved','SumQtyIncoming','SumQtyIsMultilevelSU','UnitOfMeasure','SumQty','ForPicking','ForPutaway','Blocked'],'DBTWMSItemDataCrossDocking':['Lot','ConsignmentPartner','QtyBaseUoM'],'DBTLinksTable':['Image','Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_PURCHASE_DOCUMENTFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_PURCHASE_DOCUMENTComponent, resolver);
    }
} 