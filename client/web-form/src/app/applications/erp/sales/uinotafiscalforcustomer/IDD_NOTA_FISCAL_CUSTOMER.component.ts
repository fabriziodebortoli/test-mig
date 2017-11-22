import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_NOTA_FISCAL_CUSTOMERService } from './IDD_NOTA_FISCAL_CUSTOMER.service';

@Component({
    selector: 'tb-IDD_NOTA_FISCAL_CUSTOMER',
    templateUrl: './IDD_NOTA_FISCAL_CUSTOMER.component.html',
    providers: [IDD_NOTA_FISCAL_CUSTOMERService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_NOTA_FISCAL_CUSTOMERComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_DOC_NOTA_FISCAL_CODE_validators: any;
public IDC_DOC_COMPONENTS_UOM_BE_itemSource: any;
public IDC_DOC_COMPONENTS_WASTEUOM_BE_itemSource: any;
public IDC_DOC_ACTION_ON_LIFOFIFO_LOADS_itemSource: any;
public IDC_DOC_CODETYPE_SPECIFICATOR_1_itemSource: any;
public IDC_DOC_CODETYPE_SPECIFICATOR_2_itemSource: any;
public IDC_DOC_BODY_REFERENCES_MODEL_itemSource: any;
public IDC_DOC_BODY_REFERENCES_MOV_TYPE_itemSource: any;

    constructor(document: IDD_NOTA_FISCAL_CUSTOMERService,
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
        this.IDC_DOC_NOTA_FISCAL_CODE_validators = [
  {
    "name": "NotaFiscalTypeCombo",
    "namespace": "Validator.erp.Business_BR.Components.NotaFiscalTypeCombo"
  }
]; 
this.IDC_DOC_COMPONENTS_UOM_BE_itemSource = {
  "name": "UoMSaleDocComponentComboBox",
  "namespace": "ERP.BillOfMaterials.AddOnsSales.UoMSaleDocComponentComboBox"
}; 
this.IDC_DOC_COMPONENTS_WASTEUOM_BE_itemSource = {
  "name": "UoMSaleDocComponentComboBox",
  "namespace": "ERP.BillOfMaterials.AddOnsSales.UoMSaleDocComponentComboBox"
}; 
this.IDC_DOC_ACTION_ON_LIFOFIFO_LOADS_itemSource = {
  "name": "ActionOnLifoFifoCombo",
  "namespace": "ERP.Inventory.Components.ActionOnLifoFifoReasonCombo"
}; 
this.IDC_DOC_CODETYPE_SPECIFICATOR_1_itemSource = {
  "name": "Specificator1TypeCombo",
  "namespace": "ERP.Inventory.Components.SpecTypeNoIgnoreCombo"
}; 
this.IDC_DOC_CODETYPE_SPECIFICATOR_2_itemSource = {
  "name": "Specificator2TypeCombo",
  "namespace": "ERP.Inventory.Components.SpecTypeNoIgnoreCombo"
}; 
this.IDC_DOC_BODY_REFERENCES_MODEL_itemSource = {
  "name": "NotaFiscalRefModelComboBox",
  "namespace": "ERP.Business_BR.Components.NotaFiscalRefModelComboBox"
}; 
this.IDC_DOC_BODY_REFERENCES_MOV_TYPE_itemSource = {
  "name": "MovementTypeComboBox",
  "namespace": "ERP.Business_BR.Components.MovementTypeComboBox"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'DBTNotaFiscalForCustomer':['NotaFiscalCode','Model','Series','ThirdParties','InventoryReasonAdjust','PostedToRomaneio','CancReason','CustPresenceIndicator','SimplesMsg','SimplesZeroMsg','Message1','Message2','ApproxTaxesMsg'],'HKLBRNotaFiscalType':['Description'],'SaleDocument':['DocNo','DocumentDate','PostingDate','CustSupp','Payment','PriceList','NetOfTax','InstallmStartDate','AccTpl','InvoicingAccTpl','TaxJournal','InvoicingTaxJournal','InvoicingAccGroup','ContractCode','ProjectCode','InvoiceTypes','NoChangeExigibility','TaxCommunicationGroup','TaxCommunicationOperation','PureJECollectionPaymentNo','Job','CostCenter','ProductLine','CustomerBank','CompanyBank','CompanyCA','Presentation','CompanyPymtCA','BankAuthorization','InvoicingCustomer','SendDocumentsTo','PaymentAddress','InvRsn','StubBook','ActionOnLifoFifo','StoragePhase1','Specificator1Type','SpecificatorPhase1','SpecificatorPhase1','StoragePhase2','Specificator2Type','SpecificatorPhase2','SpecificatorPhase2','OurReference','YourReference','YourReference','Notes','FromExternalProgram','PreprintedDocNo','Language'],'HKLCustomer':['CompNameCompleteWithTaxNumber'],'global':['HomologationGauge','StatusHomologation','CreditLimitGauge','CreditLimitGaugeDescri','ImageStatusSaleDoc','Detail','DBTSaleDocComponents','PymtSchedule','SalesPymtInstallmentsTot','SalesPymtDelta','SalesPymtIntallmentRegenerate','AmountRegenerate','PymtScheduleSearch','PymtScheduleDescri','AdvanceInstallmentSearch','AdvanceInstallmentDescri','DocShipToDescri','BillsShipToDescri','PickUpPointDescri','DeliveryToDescri','NFKeyDescri','bCompleteFiscalMessage','DBTNotaFiscalForCustomerRef','ManageSalespersonComm','ManageAreaManagerComm','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg','ItemDataBookInventoryActual','ItemDataBookInvDelta','ItemDataFinalBookInv','ItemDataBookInvDelta','ItemDataFinalBookInv','ItemDataBookInvValue','ItemDataOnHandActual','ItemDataDeltaOnHand','ItemDataFinalOnHand','ItemDataDeltaOnHand','ItemDataFinalOnHand','ItemDataOrderedSupplierActual','ItemDataOrderedSupplierDelta','ItemDataOrderedSupplierIsFinal','ItemDataOrderedSupplierDelta','ItemDataOrderedSupplierIsFinal','ItemDataActInProduction','ItemDataDeltaInProduction','ItemDataFinInProduction','ItemDataDeltaInProduction','ItemDataFinInProduction','ItemDataActCustReserved','ItemDataDeltaCustReserved','ItemDataFinCustReserved','ItemDataDeltaCustReserved','ItemDataFinCustReserved','ItemDataActualAllocated','ItemDataDeltaAllocated','ItemDataFinalAllocated','ItemDataDeltaAllocated','ItemDataFinalAllocated','ItemDataActReservedByProd','ItemDataDeltaReservedByProd','ItemDataFinReservedByProd','ItemDataDeltaReservedByProd','ItemDataFinReservedByProd','ItemDataActualAvailaibility','ItemDataFinalAvailaibility','ItemDataFinalAvailaibility','ItemDataKind','ItemDataValueWith','ItemDataNetWeight','ItemDataGrossWeight','ItemDataMinimumStock','ItemDataMaxStock','ItemDataReorderingLot','ItemDataProductionLot','ItemDataItemPicture','ItemDataStorage','ItemDataStorageBookInventoryActual','ItemDataStorageBookInvDelta','ItemDataStorageBookInvFinal','ItemDataStorageBookInvDelta','ItemDataStorageBookInvFinal','ItemDataStorageBookInvValue','ItemDataActualStorageQty','ItemDataDeltaStorageQty','ItemDataFinalStorageQty','ItemDataDeltaStorageQty','ItemDataFinalStorageQty','ItemDataActualSupplierStorageOrdered','ItemDataDeltaSupplierStorageOrdered','ItemDataFinalSupplierStorageOrdered','ItemDataDeltaSupplierStorageOrdered','ItemDataFinalSupplierStorageOrdered','ItemDataStorageProdInvActual','ItemDataStorageProdInvDelta','ItemDataStorageProdInvFinal','ItemDataStorageProdInvDelta','ItemDataStorageProdInvFinal','ItemDataActCustStorageReserved','ItemDataDeltaCustStorageReserved','ItemDataFinCustStorageReserved','ItemDataDeltaCustStorageReserved','ItemDataFinCustStorageReserved','ItemDataActualStorageAllocated','ItemDataDeltaStorageAllocated','ItemDataFinalStorageAllocated','ItemDataDeltaStorageAllocated','ItemDataFinalStorageAllocated','ItemDataStorageActReservedByProd','ItemDataStorageDeltaReservedByProd','ItemDataStorageFinReservedByProd','ItemDataStorageDeltaReservedByProd','ItemDataStorageFinReservedByProd','ItemDataActualStorageAvailaibility','ItemDataFinalStorageAvailaibility','ItemDataFinalStorageAvailaibility','ItemDataStorageMinimumStock','ItemDataStorageMaximumStock','ItemDataOnHandActual','ItemDataFinalOnHand','ItemDataMinimumStock','AvailabilityAnalysisDetails','LifoFifoHistory','ManItemDataDrawing','ManItemDataInProcessBOM','ManItemDataNotes','ManItemDataMRPPolicy','ManItemDataLeadTime','ManItemDataOrderReleaseDays','ManItemDataRMCost','ManItemDataSetupCost','ManItemDataInHouseProcessingCost','ManItemDataOutsourcedProcessingCost','ManItemDataProductionCost','ManItemDataProductionCostLastChange','ManItemDataItemData_LastCost','ManItemDataMO','ManItemDataStandardCost','DBTManItemDataManufacturing','ManItemDatabTreeView','DBTManItemDataTechnicalData','WMSItemDataStorage','WMSItemDataForPutawayByZone','WMSItemDataForPickingByZone','WMSItemDataBlockedByZone','DBTWMSItemDataZone','WMSItemDataForPutawayByBin','WMSItemDataForPickingByBin','WMSItemDataBlockedByBin','DBTWMSItemDataBin','WMSItemDataForPutawayByStock','WMSItemDataForPickingByStock','WMSItemDataBlockedByStock','DBTWMSItemDataStocks','WMSItemDataCrossDockingTotal','DBTWMSItemDataCrossDocking','DBTLinksTable','ImageEmpty','ImageNotaFiscal','ImageInProcess','ImageOk','ImageBlocked','ImageDisable','ImageWarning','ImageCancel','ImageError'],'HKLPaymentTerms':['Description'],'HKLPriceLists':['Description'],'Shipping':['DepartureDate','DepartureHr','DepartureMn','DepartureMn','NoOfPacks','NetWeight','GrossVolume','GrossWeight','Carrier1','Carrier2','Carrier3','Transport','Package','PackageDescription','Appearance','Vehicle','Trailer'],'Detail':['ModifiableLineBmp'],'__DBTDetail':['__CommCtg','__AreaManagerCommCtg','__CommPerc','__SalespersonDiscount','__AreaManagerCommPerc','__SalespersonCommAmount','__SalespersonBaseAmount','__AreaManagerBaseAmount','__AreaManagerCommAmount'],'Charges':['FreeSamplesDocCurr','DiscountFormula','Discounts','TotalAmount','PayableAmount','ShippingCharges','InsuranceCharges','AdditionalCharges','GoodsAmount','ServiceAmounts','FreeSamplesDocCurr','FreeSamples','DiscountOnGoods','Discounts','DiscountOnServices','DiscountFormula','GeneralDiscountTot','TotalAmount','TotalAmountDocCurr','Advance','Allowances','WithholdingTax','PayableAmount','PayableAmountInBaseCurr','PayableAmount','CreditNotePreviousPeriod','ENASARCOSalesPerc','ENASARCOSalesAmount','ReturnedMaterial','CashOnDeliveryPercentage','CashOnDeliveryCharges','PostAdvancesToAcc','PaymentTerm','Advance','AdvanceOffset','Advance2','AdvanceOffset2','Advance3','AdvanceOffset3','PrePayedAdvance'],'DBTSaleDocComponents':['Line','Component','Description','UoM','NeededQty','TotalQty','UnitValue','AccountingType','WasteUoM','NeededQtyWaste','ScrapTotalQty','Waste','WasteUnitValue'],'DBTNotaFiscalForCustomerSummary':['II_Value','IPI_Value','PIS_Value','COFINS_Value','SIMPLES_Value','SUFRAMA_Value','ICMS_Value','ICMSST_Value','ICMSEx_Value','ICMSDef_Value','Services_IR_Value','ISSTaxRateCode','Services_ISS_Value','Services_INSS_Value','Services_CS_Value','Services_PIS_Value','Services_COFINS_Value','DeductionISS','DeductionReason','ProductValue_VProd','DiscountValue_VDesc','AdvancePymtCash','AdvancePymtAccount'],'PymtSchedule':['InstallmentNo','InstallmentType','DueDate','DayName','InstallmentTot','InstallmentAmount','InstallmentTaxAmount','PymtCash','PymtAccount','CostCenter'],'HKLAccTpl':['Description'],'HKLAccTplDocSumm':['Description'],'HKLJournal':['Description'],'HKLJournalDocSumm':['Description'],'HKLAccGroupDocSumm':['Description'],'HKLContractCodes':['Description'],'HKLProjectCodes':['Description'],'HKLInvoiceType':['Description'],'HKLTaxCommunicationGroup':['Description'],'HKLAdvanceOffset':['Description'],'HKLAdvanceOffset2':['Description'],'HKLAdvanceOffset3':['Description'],'HKLJobs':['Description'],'HKLCostCenters':['Description'],'HKLProductLine':['Description'],'HKLCustomerBank':['Description'],'HKLCompanyBank':['Description'],'HKLCustInvoice':['CompanyName'],'HKLInvEntr':['Description'],'HKLStubBooks':['Description'],'HKLStorageF1':['Description'],'HKLSpecificatorF1':['CompanyName','CompanyName'],'HKLStorageF2':['Description'],'HKLSpecificatorF2':['CompanyName','CompanyName'],'HKLInvEntrAdjust':['Description'],'HKLCarrier_1':['CompNameComplete'],'HKLCarrier_2':['CompNameComplete'],'HKLCarrier_3':['CompNameComplete'],'HKLTransportMode':['Description'],'HKLGoodsAppearance':['Description'],'HKLBRVehicleTractor':['Description'],'HKLBRVehicleTrailer':['Description'],'DBTNotaFiscalForCustomerShipping':['ShippingFederalState','ShippingPlace','SpecificShippingPlace','PickUpPointType','PickUpPointCode','PickUpPointBranch','DeliveryToType','DeliveryToCode','DeliveryToBranch'],'Notes':['Notes'],'HKLLanguages':['Description'],'DBTNotaFiscalForCustomerRef':['Type','RefKey','Series','NumberNF','ModelNF','UF','DocDate','CustSuppType','CustSupp','NaturalPerson','FiscalCode','TaxIdNumber','FedStateReg','NumberECF','NumberCOO','Model','MovementType','CustRef_MoveTypeDescri','ThirdParties'],'__DBT':['__Salesperson','__SalespersonCommPolicyCode','__SalespersonBaseAmount','__SalespersonCommPerc','__SalespersonCommAmountTot','__Accrual','__AccrualPercAtInvoiceDate','__Area','__AreaManager','__AreaManagerCommPolicyCode','__AreaManagerBaseAmount','__AreaManagerCommPerc','__AreaManagerCommAmountTot'],'HKLSalesperson':['Name'],'HKLArea':['Description'],'HKLAreaManager':['Name'],'AvailabilityAnalysisDetails':['FromDate','IssuedQuantityBaseUoM','PickedQuantityBaseUoM','ProgrBalanceQty','CodeType','DocumentNumber','Line','CustSupp','CompanyName','Job','UoM','DocQty'],'LifoFifoHistory':['ReceiptBatchId','Storage','AccountingType','InvEntryType','PostingDate','Qty','LineCost'],'DBTManItemDataManufacturing':['DeliveryDate','MONo','UoM','ProductionQty','MOStatus'],'DBTManItemDataTechnicalData':['Name','NumberValue','StringValue','DateValue','BoolValue','Notes'],'DBTWMSItemDataZone':['Zone','ZoneDescri','ConsignmentPartner','AvailableQty','SumQtyBaseUoM','SumQtyReserved','SumQtyIncoming','SumQtyIsMultilevelSU','ForPicking','ForPutaway','Blocked','SpecialStock'],'DBTWMSItemDataBin':['Zone','ZoneDescri','Bin','ConsignmentPartner','AvailableQty','SumQtyBaseUoM','SumQtyReserved','SumQtyIncoming','SumQtyIsMultilevelSU','ForPicking','ForPutaway','Blocked','SpecialStock'],'DBTWMSItemDataStocks':['Zone','ZoneDescri','Bin','Lot','LotValidTo','SpecialStock','ConsignmentPartner','AvailableQty','SumQtyBaseUoM','SumQtyReserved','SumQtyIncoming','SumQtyIsMultilevelSU','UnitOfMeasure','SumQty','ForPicking','ForPutaway','Blocked'],'DBTWMSItemDataCrossDocking':['Lot','ConsignmentPartner','QtyBaseUoM'],'DBTLinksTable':['Image','Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_NOTA_FISCAL_CUSTOMERFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_NOTA_FISCAL_CUSTOMERComponent, resolver);
    }
} 