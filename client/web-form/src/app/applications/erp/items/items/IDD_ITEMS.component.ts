import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_ITEMSService } from './IDD_ITEMS.service';

@Component({
    selector: 'tb-IDD_ITEMS',
    templateUrl: './IDD_ITEMS.component.html',
    providers: [IDD_ITEMSService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_ITEMSComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_BARCODE_UOM_itemSource: any;
public IDC_ITEMS_STORAGES_CODETYPE_SPECIFICATOR_itemSource: any;
public IDC_BR_ITEM_ADDONSITEM_TAX_TYPE_itemSource: any;
public IDC_GOODS_ISSUE_UOM_itemSource: any;
public IDC_GOODS_ISSUEUOM_itemSource: any;
public IDC_GOODS_REPORT_UOM_itemSource: any;
public IDC_GOODS_PACKSUOMISSUETOPROD_itemSource: any;
public IDC_GOODS_PACKSUOMISSUE_itemSource: any;
public IDC_INTRASTAT_COUNTYOFORIGIN_itemSource: any;
public IDC_ITM_TS_CHARGE_TYPE_itemSource: any;
public IDC_ITEM_WMS_SUT_SHIPPING_UOM_itemSource: any;

    constructor(document: IDD_ITEMSService,
        eventData: EventDataService,
        resolver: ComponentFactoryResolver,
        private store: Store,
        ciService: ComponentInfoService,
        private changeDetectorRef: ChangeDetectorRef) {
        super(document, eventData, resolver, ciService);
        this.eventData.change.subscribe(() => this.changeDetectorRef.detectChanges());
    }

    ngOnInit() {
        super.ngOnInit();
        this.IDC_BARCODE_UOM_itemSource = {
  "name": "UnitsOfMeasureFromItmDocComboBox",
  "namespace": "ERP.Items.Documents.UnitsOfMeasureFromItmDocComboBox"
}; 
this.IDC_ITEMS_STORAGES_CODETYPE_SPECIFICATOR_itemSource = {
  "name": "SpecificatorTypeCombo",
  "namespace": "ERP.Inventory.Components.SpecTypeNoIgnoreCombo"
}; 
this.IDC_BR_ITEM_ADDONSITEM_TAX_TYPE_itemSource = {
  "name": "TaxTypeCombo",
  "namespace": "ERP.Items.Documents.BRTaxTypeFiscalValuesEnumCombo"
}; 
this.IDC_GOODS_ISSUE_UOM_itemSource = {
  "name": "UnitsOfMeasureFromItmDocComboBox",
  "namespace": "ERP.Items.Documents.UnitsOfMeasureFromItmDocComboBox"
}; 
this.IDC_GOODS_ISSUEUOM_itemSource = {
  "name": "UnitsOfMeasureFromItmDocComboBox",
  "namespace": "ERP.Items.Documents.UnitsOfMeasureFromItmDocComboBox"
}; 
this.IDC_GOODS_REPORT_UOM_itemSource = {
  "name": "UnitsOfMeasureFromItmDocComboBox",
  "namespace": "ERP.Items.Documents.UnitsOfMeasureFromItmDocComboBox"
}; 
this.IDC_GOODS_PACKSUOMISSUETOPROD_itemSource = {
  "name": "UnitsOfMeasureFromItmDocComboBox",
  "namespace": "ERP.Items.Documents.UnitsOfMeasureFromItmDocComboBox"
}; 
this.IDC_GOODS_PACKSUOMISSUE_itemSource = {
  "name": "UnitsOfMeasureFromItmDocComboBox",
  "namespace": "ERP.Items.Documents.UnitsOfMeasureFromItmDocComboBox"
}; 
this.IDC_INTRASTAT_COUNTYOFORIGIN_itemSource = {
  "name": "ItemSourceXml",
  "namespace": "ItemSource.Framework.TbGes.TbGes.ItemSourceXml",
  "parameter": "DataFile.ERP.Company.County"
}; 
this.IDC_ITM_TS_CHARGE_TYPE_itemSource = {
  "name": "TSChargeTypeCombo",
  "namespace": "ERP.TESANConnector.AddOnsItems.TSChargeTypeCombo"
}; 
this.IDC_ITEM_WMS_SUT_SHIPPING_UOM_itemSource = {
  "name": "UnitsOfMeasureFromItmDocComboBox",
  "namespace": "ERP.Items.Documents.UnitsOfMeasureFromItmDocComboBox"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'Items':['Item','Disabled','Description','IsGood','Draft','BaseUoM','Nature','CreationDate','ModificationDate','AvailabilityDate','OldItem','NotPostable','UseSerialNo','AdditionalCharge','Picture','Picture','SaleBarCode','BarcodeSegment','FiscalUoM','NoUoMSearch','BasePrice','BasePriceWithTax','DiscountFormula','Markup','NoAddDiscountInSaleDoc','TaxCode','SaleType','PurchaseType','HasCustomers','HasSuppliers','ItemCodes','CommissionCtg','SalespersonComm','AccountingType','SaleOffset','PurchaseOffset','ConsuptionOffset','SubjectToWithholdingTax','Job','CostCenter','ProductLine','ReverseCharge','RCTaxCode','Producer','ProductCtg','ProductSubCtg','CommodityCtg','HomogeneousCtg','ItemType','TSChargeType','TSChargeTypeDescri','TSChargeTypeFlag','TSChargeTypeFlagDescri','PublicNote','DescriptionText','InternalNote','ShortDescription'],'GoodsData':['MinimumStock','MaximumStock','ReorderingLotSize','Supplier','LastReceiptDate','LastSupplier','NoOfPacks','GrossVolume','GrossWeight','NetWeight','MinimumSaleQty','MaxUnsoldMonths','Appearance','Department','Location','LastIssueDate','OnInventorySheets','OnInventoryLevel','ReceiptUoM','IssueUoM','ReportUoM','PacksReceiptUoM','PacksIssueUoM','WEEECtg','WEEECtg2','WEEEAmount','WEEEAmount2'],'HKLSupplierPreferred':['CompanyName'],'global':['WeekDayNameReceiptDate','WeekDayNameIssueDate','PurchaseBarCode','DBTItemsStorages','BRFiscalCtgItems','ItemsBRFiscalValues','AlternativeUoM','PriceLists','Notes','__Languages','RetailPrices','WMSItemsWMSZones','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg','DBTLinksTable','ItemDataBookInventoryActual','ItemDataBookInvDelta','ItemDataFinalBookInv','ItemDataBookInvDelta','ItemDataFinalBookInv','ItemDataBookInvValue','ItemDataOnHandActual','ItemDataDeltaOnHand','ItemDataFinalOnHand','ItemDataDeltaOnHand','ItemDataFinalOnHand','ItemDataOrderedSupplierActual','ItemDataOrderedSupplierDelta','ItemDataOrderedSupplierIsFinal','ItemDataOrderedSupplierDelta','ItemDataOrderedSupplierIsFinal','ItemDataActInProduction','ItemDataDeltaInProduction','ItemDataFinInProduction','ItemDataDeltaInProduction','ItemDataFinInProduction','ItemDataActCustReserved','ItemDataDeltaCustReserved','ItemDataFinCustReserved','ItemDataDeltaCustReserved','ItemDataFinCustReserved','ItemDataActualAllocated','ItemDataDeltaAllocated','ItemDataFinalAllocated','ItemDataDeltaAllocated','ItemDataFinalAllocated','ItemDataActReservedByProd','ItemDataDeltaReservedByProd','ItemDataFinReservedByProd','ItemDataDeltaReservedByProd','ItemDataFinReservedByProd','ItemDataActualAvailaibility','ItemDataFinalAvailaibility','ItemDataFinalAvailaibility','ItemDataKind','ItemDataValueWith','ItemDataNetWeight','ItemDataGrossWeight','ItemDataMinimumStock','ItemDataMaxStock','ItemDataReorderingLot','ItemDataProductionLot','ItemDataItemPicture','ItemDataStorage','ItemDataStorageBookInventoryActual','ItemDataStorageBookInvDelta','ItemDataStorageBookInvFinal','ItemDataStorageBookInvDelta','ItemDataStorageBookInvFinal','ItemDataStorageBookInvValue','ItemDataActualStorageQty','ItemDataDeltaStorageQty','ItemDataFinalStorageQty','ItemDataDeltaStorageQty','ItemDataFinalStorageQty','ItemDataActualSupplierStorageOrdered','ItemDataDeltaSupplierStorageOrdered','ItemDataFinalSupplierStorageOrdered','ItemDataDeltaSupplierStorageOrdered','ItemDataFinalSupplierStorageOrdered','ItemDataStorageProdInvActual','ItemDataStorageProdInvDelta','ItemDataStorageProdInvFinal','ItemDataStorageProdInvDelta','ItemDataStorageProdInvFinal','ItemDataActCustStorageReserved','ItemDataDeltaCustStorageReserved','ItemDataFinCustStorageReserved','ItemDataDeltaCustStorageReserved','ItemDataFinCustStorageReserved','ItemDataActualStorageAllocated','ItemDataDeltaStorageAllocated','ItemDataFinalStorageAllocated','ItemDataDeltaStorageAllocated','ItemDataFinalStorageAllocated','ItemDataStorageActReservedByProd','ItemDataStorageDeltaReservedByProd','ItemDataStorageFinReservedByProd','ItemDataStorageDeltaReservedByProd','ItemDataStorageFinReservedByProd','ItemDataActualStorageAvailaibility','ItemDataFinalStorageAvailaibility','ItemDataFinalStorageAvailaibility','ItemDataStorageMinimumStock','ItemDataStorageMaximumStock','ItemDataOnHandActual','ItemDataFinalOnHand','ItemDataMinimumStock','AvailabilityAnalysisDetails','LifoFifoHistory','ManItemDataDrawing','ManItemDataInProcessBOM','ManItemDataNotes','ManItemDataMRPPolicy','ManItemDataLeadTime','ManItemDataOrderReleaseDays','ManItemDataRMCost','ManItemDataSetupCost','ManItemDataInHouseProcessingCost','ManItemDataOutsourcedProcessingCost','ManItemDataProductionCost','ManItemDataProductionCostLastChange','ManItemDataItemData_LastCost','ManItemDataMO','ManItemDataStandardCost','DBTManItemDataManufacturing','ManItemDatabTreeView','DBTManItemDataTechnicalData','WMSItemDataStorage','WMSItemDataForPutawayByZone','WMSItemDataForPickingByZone','WMSItemDataBlockedByZone','DBTWMSItemDataZone','WMSItemDataForPutawayByBin','WMSItemDataForPickingByBin','WMSItemDataBlockedByBin','DBTWMSItemDataBin','WMSItemDataForPutawayByStock','WMSItemDataForPickingByStock','WMSItemDataBlockedByStock','DBTWMSItemDataStocks','WMSItemDataCrossDockingTotal','DBTWMSItemDataCrossDocking'],'HKLLastSupplier':['CompanyName'],'HKLDepartments':['Description'],'PurchaseBarCode':['BarCode','BarCodeType','UoM','Notes'],'DBTItemsStorages':['Storage','StorageDescri','SpecificatorType','Specificator','MinimumStock','MaximumStock','ReorderingLotSize','MathematicRounding'],'ItemsBRTaxes':['NCM','NVE','CEST','ANP','FCI','ServiceTypeCode','ItemType','ApproxTaxesImportPerc','StateApproxTaxesImportPerc','MunApproxTaxesImportPerc','ApproxTaxesDomesticPerc','StateApproxTaxesDomesticPerc','MunApproxTaxesDomesticPerc'],'HKLBRNCM':['Description'],'HKLBRNVE':['Description'],'HKLBRCEST':['Description'],'HKLBRANP':['Description'],'HKLBRServiceType':['Description'],'BRFiscalCtgItems':['ItemFiscalCtg'],'HKLBRItemFiscalCtg':['Description'],'ItemsBRFiscalValues':['TaxType','ValidityStartingDate','ValidityEndingDate','FederalState','FiscalValue'],'AlternativeUoM':['Notes'],'HKLTaxCode':['Description'],'HKLItemsCodes':['Description'],'HKLWEEECtg':['Description'],'HKLWEEECtg2':['Description'],'HKLCtgCommiss':['Description'],'HKLAccountingType':['Description'],'HKLSaleOffset':['Description'],'HKLOffsetPurchase':['Description'],'HKLConsuptionOffset':['Description'],'HKLJobs':['Description'],'HKLCostCenters':['Description'],'HKLProductLine':['Descriprion'],'HKLRCTaxCode':['Description'],'Intrastat':['CombinedNomenclature','CountyOfOrigin','ISOOfOrigin','SpecWeightNetMass','SuppUnitSpecWeight','CPACode','IntrastatSupplyType','SuppUnitDescription','PRODCOM'],'HKLCombinedNomenclature':['Description'],'HKLISOCountryCodes':['Description'],'HKLCPA':['Description'],'HKLProducers':['CompanyName'],'HKLProductCtg':['Description'],'HKLProductSubCtg':['Description'],'HKLCommodityCtg':['Description'],'HKLHomogeneousCtg':['Description'],'HKLItemType':['Description'],'@Languages':['__Language','__Description','__Notes','__TextDescri','__TextDescri2'],'RetailPrices':['Storage','Price','PriceWithTax'],'WMSItems':['Category','CategoryForPutaway','SUTPreShipping','SUTPreShippingQty','SUTPreShippingUoM','HazardousMaterial','PrintBarcodeInGR','ConsignmentPartner','CrossDocking','UsedInWMSMobile','ScanItemInPicking','ScanItemInPutaway'],'HKLCategory':['Description'],'HKLCategoryForPutaway':['Description'],'HKLSUTShipping':['Description'],'HKLConsignmentPartner':['CompanyName'],'WMSItemsWMSZones':['Storage','Zone','FixedBinPicking','FixedBinPutaway','MinStock','MaxStock'],'DBTLinksTable':['Image','Description'],'AvailabilityAnalysisDetails':['FromDate','IssuedQuantityBaseUoM','PickedQuantityBaseUoM','ProgrBalanceQty','CodeType','DocumentNumber','Line','CustSupp','CompanyName','Job','UoM','DocQty'],'LifoFifoHistory':['ReceiptBatchId','Storage','AccountingType','InvEntryType','PostingDate','Qty','LineCost'],'DBTManItemDataManufacturing':['DeliveryDate','MONo','UoM','ProductionQty','MOStatus'],'DBTManItemDataTechnicalData':['Name','NumberValue','StringValue','DateValue','BoolValue','Notes'],'DBTWMSItemDataZone':['Zone','ZoneDescri','ConsignmentPartner','AvailableQty','SumQtyBaseUoM','SumQtyReserved','SumQtyIncoming','SumQtyIsMultilevelSU','ForPicking','ForPutaway','Blocked','SpecialStock'],'DBTWMSItemDataBin':['Zone','ZoneDescri','Bin','ConsignmentPartner','AvailableQty','SumQtyBaseUoM','SumQtyReserved','SumQtyIncoming','SumQtyIsMultilevelSU','ForPicking','ForPutaway','Blocked','SpecialStock'],'DBTWMSItemDataStocks':['Zone','ZoneDescri','Bin','Lot','LotValidTo','SpecialStock','ConsignmentPartner','AvailableQty','SumQtyBaseUoM','SumQtyReserved','SumQtyIncoming','SumQtyIsMultilevelSU','UnitOfMeasure','SumQty','ForPicking','ForPutaway','Blocked'],'DBTWMSItemDataCrossDocking':['Lot','ConsignmentPartner','QtyBaseUoM']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_ITEMSFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_ITEMSComponent, resolver);
    }
} 