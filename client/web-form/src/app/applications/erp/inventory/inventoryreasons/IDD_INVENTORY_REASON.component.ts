import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_INVENTORY_REASONService } from './IDD_INVENTORY_REASON.service';

@Component({
    selector: 'tb-IDD_INVENTORY_REASON',
    templateUrl: './IDD_INVENTORY_REASON.component.html',
    providers: [IDD_INVENTORY_REASONService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_INVENTORY_REASONComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_INVRSN_DM_DATE_LAST_ISSUE_itemSource: any;
public IDC_INVRSN_DM_LAST_SUPP_itemSource: any;
public IDC_INVRSN_DM_DATE_LAST_ISSUETOPROD_itemSource: any;
public IDC_INVRSN_CODETYPE_SPECIFICATOR_RTGSTEP1_itemSource: any;
public IDC_INVRSN_CODETYPE_SPECIFICATOR_RTGSTEP2_itemSource: any;
public IDC_INVRSN_ADF_BOOKINV_INI_itemSource: any;
public IDC_INVRSN_ADF_BOOKINV_FIN_itemSource: any;
public IDC_INVRSN_ADF_DISP_INI_itemSource: any;
public IDC_INVRSN_ADF_DISP_FIN_itemSource: any;
public IDC_INVRSN_ADF_BOOKINV_INI_PHASE_2_itemSource: any;
public IDC_INVRSN_ADF_BOOKINV_FIN_PHASE_2_itemSource: any;
public IDC_INVRSN_ADF_DISP_INI_PHASE_2_itemSource: any;
public IDC_INVRSN_ADF_DISP_FIN_PHASE_2_itemSource: any;
public IDC_INVENTORY_REASON_LIFOFIFO_ACTION_itemSource: any;
public IDC_INVRSN_AL_DATA_ISSUETOPROD_itemSource: any;
public IDC_INVRSN_AL_PRODUCT_INTERNALLY_itemSource: any;
public IDC_INVRSN_FA_DATE_LAST_SALE_itemSource: any;
public IDC_INVRSN_FA_NO_LAST_SALE_itemSource: any;
public IDC_INVRSN_FA_QTY_LAST_SALE_itemSource: any;
public IDC_INVRSN_FA_LAST_PRICE_SALE_itemSource: any;
public IDC_INVRSN_FA_LAST_DISCOUNT_SALE_itemSource: any;
public IDC_INVRSN_FA_VAL_LAST_SALE_itemSource: any;
public IDC_INVRSN_CA_NO_DN_RETURN_itemSource: any;
public IDC_INVRSN_CA_DATE_LAST_RETURN_itemSource: any;
public IDC_INVRSN_CA_QTY_LAST_RETURN_itemSource: any;
public IDC_INVRSN_CA_VAL_LAST_RETURN_itemSource: any;
public IDC_INVRSN_FA_DATE_LAST_PURCHASE_itemSource: any;
public IDC_INVRSN_FA_NO_LAST_PURCHASE_itemSource: any;
public IDC_INVRSN_FA_QTY_LAST_PURCHASE_itemSource: any;
public IDC_INVRSN_FA_LAST_DISCOUNT_itemSource: any;
public IDC_INVRSN_FA_NO_DN_RETURN_itemSource: any;
public IDC_INVRSN_FA_DATE_LAST_RETURN_itemSource: any;
public IDC_INVRSN_FA_QTY_LAST_RETURN_itemSource: any;
public IDC_INVRSN_FCM_DATE_LAST_SALE_itemSource: any;
public IDC_INVRSN_FCM_NO_LAST_SALE_itemSource: any;
public IDC_INVRSN_FCM_QTY_LAST_SALE_itemSource: any;
public IDC_INVRSN_FCM_LAST_DISCOUNT_SALE_itemSource: any;
public IDC_INVRSN_FCM_VAL_LAST_SALE_itemSource: any;
public IDC_INVRSN_CCM_NO_DN_RETURN_itemSource: any;
public IDC_INVRSN_CCM_DATE_LAST_RETURN_itemSource: any;
public IDC_INVRSN_CCM_QTY_LAST_RETURN_itemSource: any;
public IDC_INVRSN_CCM_VAL_LAST_RETURN_itemSource: any;
public IDC_INVRSN_FCM_DATE_LAST_PURCHASE_itemSource: any;
public IDC_INVRSN_FCM_NO_LAST_PURCHASE_itemSource: any;
public IDC_INVRSN_FCM_QTY_LAST_PURCHASE_itemSource: any;
public IDC_INVRSN_FCM_LAST_DISCOUNT_itemSource: any;
public IDC_INVRSN_FCM_VAL_LAST_PURCHASE_itemSource: any;
public IDC_INVRSN_FCM_NO_DN_RETURN_itemSource: any;
public IDC_INVRSN_FCM_DATE_LAST_RETURN_itemSource: any;
public IDC_INVRSN_FCM_QTY_LAST_RETURN_itemSource: any;
public IDC_INVRSN_FCM_VAL_LAST_RETURN_itemSource: any;
public IDC_INVRSN_ACT_FIFO_BOOK_INV_VALUE_itemSource: any;
public IDC_INVRSN_ACT_LIFO_BOOK_INV_VALUE_itemSource: any;

    constructor(document: IDD_INVENTORY_REASONService,
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
        this.IDC_INVRSN_DM_DATE_LAST_ISSUE_itemSource = {
  "name": "GoodsData_LastIssueDateCombo",
  "namespace": "ERP.Inventory.Components.IgnoreUpdateEnumCombo"
}; 
this.IDC_INVRSN_DM_LAST_SUPP_itemSource = {
  "name": "GoodsData_LastSupplierCombo",
  "namespace": "ERP.Inventory.Components.IgnoreUpdateEnumCombo"
}; 
this.IDC_INVRSN_DM_DATE_LAST_ISSUETOPROD_itemSource = {
  "name": "GoodsData_LastReceiptDateCombo",
  "namespace": "ERP.Inventory.Components.IgnoreUpdateEnumCombo"
}; 
this.IDC_INVRSN_CODETYPE_SPECIFICATOR_RTGSTEP1_itemSource = {
  "name": "Specificator1TypeCombo",
  "namespace": "ERP.Inventory.Components.SpecTypeNoIgnoreCombo"
}; 
this.IDC_INVRSN_CODETYPE_SPECIFICATOR_RTGSTEP2_itemSource = {
  "name": "Specificator2TypeCombo",
  "namespace": "ERP.Inventory.Components.SpecTypeNoIgnoreCombo"
}; 
this.IDC_INVRSN_ADF_BOOKINV_INI_itemSource = {
  "name": "Balances_Phase1_InitialBookInvCombo",
  "namespace": "ERP.Inventory.Components.NoUpdateEnumCombo"
}; 
this.IDC_INVRSN_ADF_BOOKINV_FIN_itemSource = {
  "name": "Balances_Phase1_FinalBookInvCombo",
  "namespace": "ERP.Inventory.Components.NoUpdateEnumCombo"
}; 
this.IDC_INVRSN_ADF_DISP_INI_itemSource = {
  "name": "Balances_Phase1_InitialOnHandCombo",
  "namespace": "ERP.Inventory.Components.NoUpdateEnumCombo"
}; 
this.IDC_INVRSN_ADF_DISP_FIN_itemSource = {
  "name": "Balances_Phase1_FinalOnHandCombo",
  "namespace": "ERP.Inventory.Components.NoUpdateEnumCombo"
}; 
this.IDC_INVRSN_ADF_BOOKINV_INI_PHASE_2_itemSource = {
  "name": "Balances_Phase2_InitialBookInvCombo",
  "namespace": "ERP.Inventory.Components.NoUpdateEnumCombo"
}; 
this.IDC_INVRSN_ADF_BOOKINV_FIN_PHASE_2_itemSource = {
  "name": "Balances_Phase2_FinalBookInvCombo",
  "namespace": "ERP.Inventory.Components.NoUpdateEnumCombo"
}; 
this.IDC_INVRSN_ADF_DISP_INI_PHASE_2_itemSource = {
  "name": "Balances_Phase2_InitialOnHandCombo",
  "namespace": "ERP.Inventory.Components.NoUpdateEnumCombo"
}; 
this.IDC_INVRSN_ADF_DISP_FIN_PHASE_2_itemSource = {
  "name": "Balances_Phase2_FinalOnHandCombo",
  "namespace": "ERP.Inventory.Components.NoUpdateEnumCombo"
}; 
this.IDC_INVENTORY_REASON_LIFOFIFO_ACTION_itemSource = {
  "name": "ActionOnLifoFifoCombo",
  "namespace": "ERP.Inventory.Components.ActionOnLifoFifoReasonCombo"
}; 
this.IDC_INVRSN_AL_DATA_ISSUETOPROD_itemSource = {
  "name": "Lots_ReceiptDataCombo",
  "namespace": "ERP.Inventory.Components.IgnoreUpdateEnumCombo"
}; 
this.IDC_INVRSN_AL_PRODUCT_INTERNALLY_itemSource = {
  "name": "Lots_InternallyProducedCombo",
  "namespace": "ERP.Inventory.Components.IgnoreUpdateEnumCombo"
}; 
this.IDC_INVRSN_FA_DATE_LAST_SALE_itemSource = {
  "name": "ItemCustomers_LastSaleDocDateCombo",
  "namespace": "ERP.Inventory.Components.IgnoreUpdateEnumCombo"
}; 
this.IDC_INVRSN_FA_NO_LAST_SALE_itemSource = {
  "name": "ItemCustomers_LastSaleNoCombo",
  "namespace": "ERP.Inventory.Components.IgnoreUpdateEnumCombo"
}; 
this.IDC_INVRSN_FA_QTY_LAST_SALE_itemSource = {
  "name": "ItemCustomers_LastSaleQuantCombo",
  "namespace": "ERP.Inventory.Components.IgnoreUpdateEnumCombo"
}; 
this.IDC_INVRSN_FA_LAST_PRICE_SALE_itemSource = {
  "name": "ItemCustomers_LastPriceCombo",
  "namespace": "ERP.Inventory.Components.IgnoreUpdateEnumCombo"
}; 
this.IDC_INVRSN_FA_LAST_DISCOUNT_SALE_itemSource = {
  "name": "ItemCustomers_LastDiscountCombo",
  "namespace": "ERP.Inventory.Components.IgnoreUpdateEnumCombo"
}; 
this.IDC_INVRSN_FA_VAL_LAST_SALE_itemSource = {
  "name": "ItemCustomers_LastSaleValueCombo",
  "namespace": "ERP.Inventory.Components.IgnoreUpdateEnumCombo"
}; 
this.IDC_INVRSN_CA_NO_DN_RETURN_itemSource = {
  "name": "ItemCustomers_LastRMANoCombo",
  "namespace": "ERP.Inventory.Components.IgnoreUpdateEnumCombo"
}; 
this.IDC_INVRSN_CA_DATE_LAST_RETURN_itemSource = {
  "name": "ItemCustomers_LastRMADocDateCombo",
  "namespace": "ERP.Inventory.Components.IgnoreUpdateEnumCombo"
}; 
this.IDC_INVRSN_CA_QTY_LAST_RETURN_itemSource = {
  "name": "ItemCustomers_LastRMAQtyCombo",
  "namespace": "ERP.Inventory.Components.IgnoreUpdateEnumCombo"
}; 
this.IDC_INVRSN_CA_VAL_LAST_RETURN_itemSource = {
  "name": "ItemCustomers_LastReturnValueCombo",
  "namespace": "ERP.Inventory.Components.IgnoreUpdateEnumCombo"
}; 
this.IDC_INVRSN_FA_DATE_LAST_PURCHASE_itemSource = {
  "name": "ItemSupp_LastPurchaseDocDateCombo",
  "namespace": "ERP.Inventory.Components.IgnoreUpdateEnumCombo"
}; 
this.IDC_INVRSN_FA_NO_LAST_PURCHASE_itemSource = {
  "name": "ItemSupp_LastPurchaseNoCombo",
  "namespace": "ERP.Inventory.Components.IgnoreUpdateEnumCombo"
}; 
this.IDC_INVRSN_FA_QTY_LAST_PURCHASE_itemSource = {
  "name": "ItemSupp_LastPurchaseQtyCombo",
  "namespace": "ERP.Inventory.Components.IgnoreUpdateEnumCombo"
}; 
this.IDC_INVRSN_FA_LAST_DISCOUNT_itemSource = {
  "name": "ItemSupp_LastDiscountCombo",
  "namespace": "ERP.Inventory.Components.IgnoreUpdateEnumCombo"
}; 
this.IDC_INVRSN_FA_NO_DN_RETURN_itemSource = {
  "name": "ItemSupp_LastRMANoCombo",
  "namespace": "ERP.Inventory.Components.IgnoreUpdateEnumCombo"
}; 
this.IDC_INVRSN_FA_DATE_LAST_RETURN_itemSource = {
  "name": "ItemSupp_LastRMADocDateCombo",
  "namespace": "ERP.Inventory.Components.IgnoreUpdateEnumCombo"
}; 
this.IDC_INVRSN_FA_QTY_LAST_RETURN_itemSource = {
  "name": "ItemSupp_LastRMAQtyCombo",
  "namespace": "ERP.Inventory.Components.IgnoreUpdateEnumCombo"
}; 
this.IDC_INVRSN_FCM_DATE_LAST_SALE_itemSource = {
  "name": "CustCommCtg_LastSaleDocDateCombo",
  "namespace": "ERP.Inventory.Components.IgnoreUpdateEnumCombo"
}; 
this.IDC_INVRSN_FCM_NO_LAST_SALE_itemSource = {
  "name": "CustCommCtg_LastSaleNoCombo",
  "namespace": "ERP.Inventory.Components.IgnoreUpdateEnumCombo"
}; 
this.IDC_INVRSN_FCM_QTY_LAST_SALE_itemSource = {
  "name": "CustCommCtg_LastSaleQuantCombo",
  "namespace": "ERP.Inventory.Components.IgnoreUpdateEnumCombo"
}; 
this.IDC_INVRSN_FCM_LAST_DISCOUNT_SALE_itemSource = {
  "name": "CustCommCtg_LastDiscountCombo",
  "namespace": "ERP.Inventory.Components.IgnoreUpdateEnumCombo"
}; 
this.IDC_INVRSN_FCM_VAL_LAST_SALE_itemSource = {
  "name": "CustCommCtg_LastSaleValueCombo",
  "namespace": "ERP.Inventory.Components.IgnoreUpdateEnumCombo"
}; 
this.IDC_INVRSN_CCM_NO_DN_RETURN_itemSource = {
  "name": "CustCommCtg_LastRMANoCombo",
  "namespace": "ERP.Inventory.Components.IgnoreUpdateEnumCombo"
}; 
this.IDC_INVRSN_CCM_DATE_LAST_RETURN_itemSource = {
  "name": "CustCommCtg_LastRMADocDateCombo",
  "namespace": "ERP.Inventory.Components.IgnoreUpdateEnumCombo"
}; 
this.IDC_INVRSN_CCM_QTY_LAST_RETURN_itemSource = {
  "name": "CustCommCtg_LastRMAQtyCombo",
  "namespace": "ERP.Inventory.Components.IgnoreUpdateEnumCombo"
}; 
this.IDC_INVRSN_CCM_VAL_LAST_RETURN_itemSource = {
  "name": "CustCommCtg_LastReturnValueCombo",
  "namespace": "ERP.Inventory.Components.IgnoreUpdateEnumCombo"
}; 
this.IDC_INVRSN_FCM_DATE_LAST_PURCHASE_itemSource = {
  "name": "SuppCommCtg_LastPurchaseDocDateCombo",
  "namespace": "ERP.Inventory.Components.IgnoreUpdateEnumCombo"
}; 
this.IDC_INVRSN_FCM_NO_LAST_PURCHASE_itemSource = {
  "name": "SuppCommCtg_LastPurchaseNoCombo",
  "namespace": "ERP.Inventory.Components.IgnoreUpdateEnumCombo"
}; 
this.IDC_INVRSN_FCM_QTY_LAST_PURCHASE_itemSource = {
  "name": "SuppCommCtg_LastPurchaseQtyCombo",
  "namespace": "ERP.Inventory.Components.IgnoreUpdateEnumCombo"
}; 
this.IDC_INVRSN_FCM_LAST_DISCOUNT_itemSource = {
  "name": "SuppCommCtg_LastDiscountCombo",
  "namespace": "ERP.Inventory.Components.IgnoreUpdateEnumCombo"
}; 
this.IDC_INVRSN_FCM_VAL_LAST_PURCHASE_itemSource = {
  "name": "SuppCommCtg_LastPurchaseValueCombo",
  "namespace": "ERP.Inventory.Components.IgnoreUpdateEnumCombo"
}; 
this.IDC_INVRSN_FCM_NO_DN_RETURN_itemSource = {
  "name": "SuppCommCtg_LastRMANoCombo",
  "namespace": "ERP.Inventory.Components.IgnoreUpdateEnumCombo"
}; 
this.IDC_INVRSN_FCM_DATE_LAST_RETURN_itemSource = {
  "name": "SuppCommCtg_LastRMADocDateCombo",
  "namespace": "ERP.Inventory.Components.IgnoreUpdateEnumCombo"
}; 
this.IDC_INVRSN_FCM_QTY_LAST_RETURN_itemSource = {
  "name": "SuppCommCtg_LastRMAQtyCombo",
  "namespace": "ERP.Inventory.Components.IgnoreUpdateEnumCombo"
}; 
this.IDC_INVRSN_FCM_VAL_LAST_RETURN_itemSource = {
  "name": "SuppCommCtg_LastReturnValueCombo",
  "namespace": "ERP.Inventory.Components.IgnoreUpdateEnumCombo"
}; 
this.IDC_INVRSN_ACT_FIFO_BOOK_INV_VALUE_itemSource = {
  "name": "FIFO_BookInvValueCombo",
  "namespace": "ERP.Inventory.Components.IgnoreUpdateEnumCombo"
}; 
this.IDC_INVRSN_ACT_LIFO_BOOK_INV_VALUE_itemSource = {
  "name": "LIFO_BookInvValueCombo",
  "namespace": "ERP.Inventory.Components.IgnoreUpdateEnumCombo"
}; 

        		this.bo.appendToModelStructure({'InventoryReasons':['Reason','DefaultReason','Disabled','Description','CustSuppType','FiscalNumbering','NonFiscal','ShippingReason','StubBook','EditableStubBook','ProposedValue','EditableUnitValue','IsStorageTransfer','IsInventoryAdjustement','AlignSpecificator','NoChangeExigibility','SearchForBarCode','GenerateSerialNo','InBaseCurrency','InvoiceFollows','UsedEverywhere','UsedInPurchases','UsedInInventory','UsedInSales','UsedInManufacturing','CostAccounting','DebitCreditSign','CostAccSign','UseGoodsData','StoragePhase1','Specificator1Type','SpecificatorPhase1','UseStorage1','UseSpecificator1','Specificator1IsMand','UsePhase2','StoragePhase2','Specificator2Type','SpecificatorPhase2','UseStorage2','UseSpecificator2','Specificator2IsMand','ActionOnLifoFifo','LineCostOrigin','LifoFifo_LineSource','NeedCIGCorrection','CIGCorrInvReason','ExtAccountingTemplate','UseItemCustomers','UseItemSuppliers','UseCommCtgCustomers','UseCommCtgSuppliers','UseFIFOAction','UseLIFOAction'],'HKLShippingReasons':['Description'],'HKLStubBook':['Description'],'global':['OptionalData','DocNoIsMand','MandatoryQty','MandatoryValue','GoodsData_LastIssueDate','GoodsData_LastSupplier','GoodsData_LastReceiptDate','Balances_Phase1_InitialBookInv','Balances_Phase1_FinalBookInv','Balances_Phase1_BookInvInValue','Balances_Phase1_FinalBookInvValue','Balances_Phase1_InitialOnHand','Balances_Phase1_FinalOnHand','Balances_Phase1_LastCost','Balances_Phase1_PurchasedQty','Balances_Phase1_PurchaseValue','Balances_Phase1_SalesQty','Balances_Phase1_SentValue','Balances_Phase1_ScrapQty','Balances_Phase1_ScrapValue','Balances_Phase1_ReceivedQty','Balances_Phase1_ReceiptValue','Balances_Phase1_IssuedQty','Balances_Phase1_IssuedValue','Balances_Phase1_CIGValue','Balances_Phase1_InReturn','Balances_Phase1_Return','Balances_Phase1_InitialForRepairing','Balances_Phase1_ForRepairing','Balances_Phase1_InitialSampleGoods','Balances_Phase1_SampleGoods','Balances_Phase1_InitialSampling','Balances_Phase1_Sampling','Balances_Phase1_InitialBailment','Balances_Phase1_Bailment','Balances_Phase1_ReturnedFromProdQty','Balances_Phase1_ReturnFromProdValue','Balances_Phase1_UsedInInProduction','Balances_Phase1_UsedInProduction','Balances_Phase1_UsedInInProductionValue','Balances_Phase1_UsedInProductionValue','Balances_Phase1_PickingValue','Balances_Phase1_InitialSubcontracting','Balances_Phase1_Subcontracting','Balances_Phase1_InitialForSubcontracting','Balances_Phase1_ForSubcontracting','Balances_Phase1_InProduction','FreeValQty1Descri','Balances_Phase1_InitialQtyValFree1','Balances_Phase1_QtyValFree1','FreeValQty2Descri','Balances_Phase1_InitialQtyValFree2','Balances_Phase1_QtyValFree2','FreeValQty3Descri','Balances_Phase1_InitialQtyValFree3','Balances_Phase1_QtyValFree3','FreeValQty4Descri','Balances_Phase1_InitialQtyValFree4','Balances_Phase1_QtyValFree4','FreeValQty5Descri','Balances_Phase1_InitialQtyValFree5','Balances_Phase1_QtyValFree5','Balances_Phase2_InitialBookInv','Balances_Phase2_FinalBookInv','Balances_Phase2_BookInvInValue','Balances_Phase2_FinalBookInvValue','Balances_Phase2_InitialOnHand','Balances_Phase2_FinalOnHand','Balances_Phase2_LastCost','Balances_Phase2_PurchasedQty','Balances_Phase2_PurchaseValue','Balances_Phase2_SalesQty','Balances_Phase2_SentValue','Balances_Phase2_ScrapQty','Balances_Phase2_ScrapValue','Balances_Phase2_ReceivedQty','Balances_Phase2_ReceiptValue','Balances_Phase2_IssuedQty','Balances_Phase2_IssuedValue','Balances_Phase2_CIGValue','Balances_Phase2_InReturn','Balances_Phase2_Return','Balances_Phase2_InitialForRepairing','Balances_Phase2_ForRepairing','Balances_Phase2_InitialSampleGoods','Balances_Phase2_SampleGoods','Balances_Phase2_InitialSampling','Balances_Phase2_Sampling','Balances_Phase2_InitialBailment','Balances_Phase2_Bailment','Balances_Phase2_ReturnedFromProdQty','Balances_Phase2_ReturnFromProdValue','Balances_Phase2_UsedInInProduction','Balances_Phase2_UsedInProduction','Balances_Phase2_UsedInInProductionValue','Balances_Phase2_UsedInProductionValue','Balances_Phase2_PickingValue','Balances_Phase2_InitialSubcontracting','Balances_Phase2_Subcontracting','Balances_Phase2_InitialForSubcontracting','Balances_Phase2_ForSubcontracting','Balances_Phase2_InProduction','FreeValQty1DescriPhase2','Balances_Phase2_InitialQtyValFree1','Balances_Phase2_QtyValFree1','FreeValQty2DescriPhase2','Balances_Phase2_InitialQtyValFree2','Balances_Phase2_QtyValFree2','FreeValQty3DescriPhase2','Balances_Phase2_InitialQtyValFree3','Balances_Phase2_QtyValFree3','FreeValQty4DescriPhase2','Balances_Phase2_InitialQtyValFree4','Balances_Phase2_QtyValFree4','FreeValQty5DescriPhase2','Balances_Phase2_InitialQtyValFree5','Balances_Phase2_QtyValFree5','Lots_ReceiptData','Lots_InternallyProduced','ItemCustomers_LastSaleDocDate','ItemCustomers_LastSaleNo','ItemCustomers_LastSaleQuant','ItemCustomers_LastPrice','ItemCustomers_LastDiscount','ItemCustomers_LastSaleValue','ItemCustomers_LastRMANo','ItemCustomers_LastRMADocDate','ItemCustomers_LastRMAQty','ItemCustomers_LastReturnValue','ItemSupp_LastPurchaseDocDate','ItemSupp_LastPurchaseNo','ItemSupp_LastPurchaseQty','ItemSupp_LastPrice','ItemSupp_LastDiscount','ItemSupp_LastPurchaseValue','ItemSupp_LastRMANo','ItemSupp_LastRMADocDate','ItemSupp_LastRMAQty','ItemSupp_LastReturnValue','CustCommCtg_LastSaleDocDate','CustCommCtg_LastSaleNo','CustCommCtg_LastSaleQuant','CustCommCtg_LastDiscount','CustCommCtg_LastSaleValue','CustCommCtg_LastRMANo','CustCommCtg_LastRMADocDate','CustCommCtg_LastRMAQty','CustCommCtg_LastReturnValue','SuppCommCtg_LastPurchaseDocDate','SuppCommCtg_LastPurchaseNo','SuppCommCtg_LastPurchaseQty','SuppCommCtg_LastDiscount','SuppCommCtg_LastPurchaseValue','SuppCommCtg_LastRMANo','SuppCommCtg_LastRMADocDate','SuppCommCtg_LastRMAQty','SuppCommCtg_LastReturnValue','FIFO_BookInvValue','LIFO_BookInvValue','__Languages','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'HKLStoragesRtgStep1':['Description'],'HKLSpec1':['CompanyName'],'HKLStoragesRtgStep2':['Description'],'HKLSpec2':['CompanyName'],'HKLRsnInvCIG':['Description'],'HKLExtAccTempl':['Description'],'@Languages':['__Language','__Description','__Notes','__TextDescri','__TextDescri2']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_INVENTORY_REASONFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_INVENTORY_REASONComponent, resolver);
    }
} 