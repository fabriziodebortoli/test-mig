import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_NFSService } from './IDD_NFS.service';

@Component({
    selector: 'tb-IDD_NFS',
    templateUrl: './IDD_NFS.component.html',
    providers: [IDD_NFSService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_NFSComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_PURCH_DOC_NOTA_FISCAL_CODE_validators: any;
public IDC_NOTA_FISCAL_FOR_SUPPLIER_LINK_ORDERS_UOM_BE_itemSource: any;
public IDC_PURCH_DOC_CODETYPE_SPECIFICATOR_1_itemSource: any;
public IDC_PURCH_DOC_CODETYPE_SPECIFICATOR_2_itemSource: any;
public IDC_NOTA_FISCAL_FOR_SUPPLIER_REF_MODEL_itemSource: any;
public IDC_NOTA_FISCAL_FOR_SUPPLIER_REF_MOV_TYPE_itemSource: any;

    constructor(document: IDD_NFSService,
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
        this.IDC_PURCH_DOC_NOTA_FISCAL_CODE_validators = [
  {
    "name": "NotaFiscalTypeCombo",
    "namespace": "Validator.erp.Business_BR.Components.NotaFiscalTypeCombo"
  }
]; 
this.IDC_NOTA_FISCAL_FOR_SUPPLIER_LINK_ORDERS_UOM_BE_itemSource = {
  "name": "UnitsOfMeasureInvoiceComboBox",
  "namespace": "ERP.InvoiceMng.Components.UnitsOfMeasureInvoiceComboBox"
}; 
this.IDC_PURCH_DOC_CODETYPE_SPECIFICATOR_1_itemSource = {
  "name": "ConformingSpecificator1TypeCombo",
  "namespace": "ERP.Inventory.Components.SpecTypeNoIgnoreCombo"
}; 
this.IDC_PURCH_DOC_CODETYPE_SPECIFICATOR_2_itemSource = {
  "name": "ConformingSpecificator2TypeCombo",
  "namespace": "ERP.Inventory.Components.SpecTypeNoIgnoreCombo"
}; 
this.IDC_NOTA_FISCAL_FOR_SUPPLIER_REF_MODEL_itemSource = {
  "name": "NotaFiscalRefModelComboBox",
  "namespace": "ERP.Business_BR.Components.NotaFiscalRefModelComboBox"
}; 
this.IDC_NOTA_FISCAL_FOR_SUPPLIER_REF_MOV_TYPE_itemSource = {
  "name": "MovementTypeComboBox",
  "namespace": "ERP.Business_BR.Components.MovementTypeComboBox"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'DBTNotaFiscalForSupplier':['NotaFiscalCode','Model','Series','ThirdParties','InventoryReasonAdjust','CancReason','CustPresenceIndicator','SimplesMsg','SimplesZeroMsg','Message1','Message2','ApproxTaxesMsg'],'HKLBRNotaFiscalType':['Description'],'PurchaseDocument':['DocNo','DocumentDate','PostingDate','Supplier','Payment','InstallmStartDate','AccTpl','InvoicingAccGroup','ContractCode','ProjectCode','PureJECollectionPaymentNo','Job','CostCenter','ProductLine','SupplierBank','CompanyBank','CompanyCA','SupplierCA','ESRReferenceNumber','ESRCheckDigit','PaymentAddress','ConfInvRsn','StubBook','ConformingStorage1','ConformingSpecificator1Type','ConformingSpecificator1','ConformingSpecificator1','ConformingStorage2','ConformingSpecificator2Type','ConformingSpecificator2','ConformingSpecificator2','ActionOnLifoFifo','Invoiced','OurReference','YourReference','Language','Notes'],'HKLSupplier':['CompNameCompleteWithTaxNumber'],'global':['DummyGauge','StatusHomologation','DummyGauge','Detail','PurchaseDocLinkOrders','PymtSchedule','PurchPymtInstallmentsTot','PurchPymtDelta','PurchPymtIntallmentRegenerate','PurchPymtAmountRegenerate','AdvanceInstallmentSearch','AdvanceInstallmentDescri','AdvanceInstallmentDescri','SendPaymentToDescri','PickUpPointDescri','DeliveryToDescri','Notes','NFKeyDescri','bCompleteFiscalMessage','DBTNotaFiscalForSupplierRef','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg','ItemDataBookInventoryActual','ItemDataBookInvDelta','ItemDataFinalBookInv','ItemDataBookInvDelta','ItemDataFinalBookInv','ItemDataBookInvValue','ItemDataOnHandActual','ItemDataDeltaOnHand','ItemDataFinalOnHand','ItemDataDeltaOnHand','ItemDataFinalOnHand','ItemDataOrderedSupplierActual','ItemDataOrderedSupplierDelta','ItemDataOrderedSupplierIsFinal','ItemDataOrderedSupplierDelta','ItemDataOrderedSupplierIsFinal','ItemDataActInProduction','ItemDataDeltaInProduction','ItemDataFinInProduction','ItemDataDeltaInProduction','ItemDataFinInProduction','ItemDataActCustReserved','ItemDataDeltaCustReserved','ItemDataFinCustReserved','ItemDataDeltaCustReserved','ItemDataFinCustReserved','ItemDataActualAllocated','ItemDataDeltaAllocated','ItemDataFinalAllocated','ItemDataDeltaAllocated','ItemDataFinalAllocated','ItemDataActReservedByProd','ItemDataDeltaReservedByProd','ItemDataFinReservedByProd','ItemDataDeltaReservedByProd','ItemDataFinReservedByProd','ItemDataActualAvailaibility','ItemDataFinalAvailaibility','ItemDataFinalAvailaibility','ItemDataKind','ItemDataValueWith','ItemDataNetWeight','ItemDataGrossWeight','ItemDataMinimumStock','ItemDataMaxStock','ItemDataReorderingLot','ItemDataProductionLot','ItemDataItemPicture','ItemDataStorage','ItemDataStorageBookInventoryActual','ItemDataStorageBookInvDelta','ItemDataStorageBookInvFinal','ItemDataStorageBookInvDelta','ItemDataStorageBookInvFinal','ItemDataStorageBookInvValue','ItemDataActualStorageQty','ItemDataDeltaStorageQty','ItemDataFinalStorageQty','ItemDataDeltaStorageQty','ItemDataFinalStorageQty','ItemDataActualSupplierStorageOrdered','ItemDataDeltaSupplierStorageOrdered','ItemDataFinalSupplierStorageOrdered','ItemDataDeltaSupplierStorageOrdered','ItemDataFinalSupplierStorageOrdered','ItemDataStorageProdInvActual','ItemDataStorageProdInvDelta','ItemDataStorageProdInvFinal','ItemDataStorageProdInvDelta','ItemDataStorageProdInvFinal','ItemDataActCustStorageReserved','ItemDataDeltaCustStorageReserved','ItemDataFinCustStorageReserved','ItemDataDeltaCustStorageReserved','ItemDataFinCustStorageReserved','ItemDataActualStorageAllocated','ItemDataDeltaStorageAllocated','ItemDataFinalStorageAllocated','ItemDataDeltaStorageAllocated','ItemDataFinalStorageAllocated','ItemDataStorageActReservedByProd','ItemDataStorageDeltaReservedByProd','ItemDataStorageFinReservedByProd','ItemDataStorageDeltaReservedByProd','ItemDataStorageFinReservedByProd','ItemDataActualStorageAvailaibility','ItemDataFinalStorageAvailaibility','ItemDataFinalStorageAvailaibility','ItemDataStorageMinimumStock','ItemDataStorageMaximumStock','ItemDataOnHandActual','ItemDataFinalOnHand','ItemDataMinimumStock','AvailabilityAnalysisDetails','LifoFifoHistory','ManItemDataDrawing','ManItemDataInProcessBOM','ManItemDataNotes','ManItemDataMRPPolicy','ManItemDataLeadTime','ManItemDataOrderReleaseDays','ManItemDataRMCost','ManItemDataSetupCost','ManItemDataInHouseProcessingCost','ManItemDataOutsourcedProcessingCost','ManItemDataProductionCost','ManItemDataProductionCostLastChange','ManItemDataItemData_LastCost','ManItemDataMO','ManItemDataStandardCost','DBTManItemDataManufacturing','ManItemDatabTreeView','DBTManItemDataTechnicalData','WMSItemDataStorage','WMSItemDataForPutawayByZone','WMSItemDataForPickingByZone','WMSItemDataBlockedByZone','DBTWMSItemDataZone','WMSItemDataForPutawayByBin','WMSItemDataForPickingByBin','WMSItemDataBlockedByBin','DBTWMSItemDataBin','WMSItemDataForPutawayByStock','WMSItemDataForPickingByStock','WMSItemDataBlockedByStock','DBTWMSItemDataStocks','WMSItemDataCrossDockingTotal','DBTWMSItemDataCrossDocking','DBTLinksTable','ImageEmpty','ImageNotaFiscal','ImageInProcess','ImageOk','ImageBlocked','ImageDisable','ImageWarning','ImageCancel','ImageError'],'HKLPaymentTerms':['Description'],'Shipping':['DepartureDate','DepartureHr','DepartureMn','DepartureMn','NoOfPacks','NetWeight','GrossVolume','GrossWeight','Carrier1','Carrier2','Carrier3','Package','PackageDescription','Transport','Appearance','Vehicle','Trailer'],'Detail':['l_ModifiableLineBmp','SupplierLotNo','SupplierLotExpiryDate'],'PurchaseDocLinkOrders':['Line','PurchaseOrdNo','PurchaseOrdPos','ClosePurchaseOrd','UoM','Quantity','l_PurchaseOrdUoM','l_PurchaseOrdQty'],'Charges':['FreeSamplesDocCurr','DiscountFormula','Discounts','TotalAmount','PayableAmount','ShippingCharges','InsuranceCharges','AdditionalCharges','GoodsAmount','ServiceAmounts','FreeSamplesDocCurr','DiscountOnGoods','Discounts','DiscountOnServices','DiscountFormula','GeneralDiscountTot','TotalAmount','TotalAmountDocCurr','Advance','Allowances','PayableAmount','PayableAmountInBaseCurr','PayableAmount','CashOnDeliveryPercentage','CashOnDeliveryCharges','PostAdvancesToAcc','PaymentTerm','Advance','AdvanceOffset','AdvanceOffset','PrePayedAdvance','PrePayedAdvance'],'DBTNotaFiscalForSupplierSummary':['II_Value','IPI_Value','PIS_Value','COFINS_Value','SIMPLES_Value','SUFRAMA_Value','ICMS_Value','ICMSST_Value','ICMSEx_Value','ICMSDef_Value','Services_IR_Value','ISSTaxRateCode','Services_ISS_Value','Services_INSS_Value','Services_CS_Value','Services_PIS_Value','Services_COFINS_Value','DeductionISS','DeductionReason','ProductValue_VProd','DiscountValue_VDesc','AdvancePymtCash','AdvancePymtAccount'],'PymtSchedule':['InstallmentNo','InstallmentType','DueDate','DayName','InstallmentTot','InstallmentAmount','InstallmentTaxAmount','PymtCash','PymtAccount','CostCenter'],'HKLAccTpl':['Description'],'HKLAccGroupDocSumm':['Description'],'HKLContractCodes':['Description'],'HKLProjectCodes':['Description'],'HKLAdvanceOffset':['Description','Description'],'HKLJobs':['Description'],'HKLCostCenters':['Description'],'HKLProductLine':['Description'],'HKLSupplierBank':['Description'],'HKLCompanyBank':['Description'],'HKLInvEntr':['Description'],'HKLStubBook':['Description'],'HKLStorageF1':['Description'],'HKLSpecificatorF1':['CompanyName','CompanyName'],'HKLStorageF2':['Description'],'HKLSpecificatorF2':['CompanyName','CompanyName'],'HKLInvEntrAdjust':['Description'],'HKLCarrier_1':['CompNameComplete'],'HKLCarrier_2':['CompNameComplete'],'HKLCarrier_3':['CompNameComplete'],'HKLTransport':['Description'],'HKLGoodsAppearance':['Description'],'HKLBRVehicleTractor':['Description'],'HKLBRVehicleTrailer':['Description'],'DBTNotaFiscalForSupplierShipping':['PickUpPointType','PickUpPointCode','PickUpPointBranch','DeliveryToType','DeliveryToCode','DeliveryToBranch'],'HKLLanguages':['Description'],'Notes':['Line','Notes','ChangeDate','GenByConsistencyCheck'],'DBTNotaFiscalForSupplierRef':['Type','RefKey','Series','NumberNF','ModelNF','UF','DocDate','CustSuppType','CustSupp','NaturalPerson','FiscalCode','TaxIdNumber','FedStateReg','NumberECF','NumberCOO','Model','MovementType','l_SuppRef_MoveTypeDescri','ThirdParties'],'AvailabilityAnalysisDetails':['FromDate','IssuedQuantityBaseUoM','PickedQuantityBaseUoM','ProgrBalanceQty','CodeType','DocumentNumber','Line','CustSupp','CompanyName','Job','UoM','DocQty'],'LifoFifoHistory':['ReceiptBatchId','Storage','AccountingType','InvEntryType','PostingDate','Qty','LineCost'],'DBTManItemDataManufacturing':['DeliveryDate','MONo','UoM','ProductionQty','MOStatus'],'DBTManItemDataTechnicalData':['Name','NumberValue','StringValue','DateValue','BoolValue','Notes'],'DBTWMSItemDataZone':['Zone','ZoneDescri','ConsignmentPartner','AvailableQty','SumQtyBaseUoM','SumQtyReserved','SumQtyIncoming','SumQtyIsMultilevelSU','ForPicking','ForPutaway','Blocked','SpecialStock'],'DBTWMSItemDataBin':['Zone','ZoneDescri','Bin','ConsignmentPartner','AvailableQty','SumQtyBaseUoM','SumQtyReserved','SumQtyIncoming','SumQtyIsMultilevelSU','ForPicking','ForPutaway','Blocked','SpecialStock'],'DBTWMSItemDataStocks':['Zone','ZoneDescri','Bin','Lot','LotValidTo','SpecialStock','ConsignmentPartner','AvailableQty','SumQtyBaseUoM','SumQtyReserved','SumQtyIncoming','SumQtyIsMultilevelSU','UnitOfMeasure','SumQty','ForPicking','ForPutaway','Blocked'],'DBTWMSItemDataCrossDocking':['Lot','ConsignmentPartner','QtyBaseUoM'],'DBTLinksTable':['Image','Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_NFSFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_NFSComponent, resolver);
    }
} 