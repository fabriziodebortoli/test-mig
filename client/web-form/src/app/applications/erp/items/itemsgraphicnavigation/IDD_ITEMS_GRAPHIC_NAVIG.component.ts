import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_ITEMS_GRAPHIC_NAVIGService } from './IDD_ITEMS_GRAPHIC_NAVIG.service';

@Component({
    selector: 'tb-IDD_ITEMS_GRAPHIC_NAVIG',
    templateUrl: './IDD_ITEMS_GRAPHIC_NAVIG.component.html',
    providers: [IDD_ITEMS_GRAPHIC_NAVIGService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_ITEMS_GRAPHIC_NAVIGComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_ITEMS_GRAPHIC_NAVIGService,
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
        
        		this.bo.appendToModelStructure({'global':['pHelperAllMO','pHelperMOSel','pHelperMOStatus','HFItems_All','HFItems_Range','HFItems_From','HFItems_To','pHelperItem_Code','pHelperItem_ItemDescri','pHelperItem_BasePrice','pHelperItem_StandardPriceWithTax','pHelperItem_BaseUoM','pHelperItem_DiscountFormula','pHelperItem_Markup','FinalBookInventory','FinalOnHand','OrderedPurchOrd','OrderedToProd','ReservedSaleOrd','AllocatedQty','ReservedByProd','Availability','ItemPriceListsDetails','ItemComparableUoMDetails','ItemSubstituteItemsDetails','ItemWMSDetails','pHelperItemCust_BasePrice','pHelperItemCust_StandardPriceWithTax','pHelperItemCust_BaseUoM','pHelperItemCust_Currency','pHelperItemCust_DiscountFormula','pHelperItemCust_LastDocType','pHelperItemCust_LastSaleDocNo','pHelperItemCust_LastSaleDate','pHelperItemCust_LastPriceCurrency','pHelperItemCust_LastPriceCurrencyDescri','pHelperItemCust_LastPayment','pHelperItemCust_Currency','pHelperItemCust_LastPriceUoM','pHelperItemCust_LastSaleQty','pHelperItemCust_LastBasePrice','pHelperItemCust_LastPriceWithTax','pHelperItemCust_LastDiscountFormula','pHelperItemCust_LastSaleValue','pHelperItemCust_LastRMADocNo','pHelperItemCust_LastRMADocDate','pHelperItemCust_LastRMAQty','pHelperItemCust_LastRMAValue','ItemCustomersDetails','pHelperItemSupp_BasePrice','pHelperItemSupp_StandardPriceWithTax','pHelperItemSupp_BaseUoM','pHelperItemSupp_DiscountFormula','pHelperItemSupp_LastDocType','pHelperItemSupp_LastPurchaseDocNo','pHelperItemSupp_LastPurchaseDate','pHelperItemSupp_LastPriceCurrency','pHelperItemSupp_LastPriceCurrencyDescri','pHelperItemSupp_LastPayment','pHelperItemSupp_LastPaymentDescri','pHelperItemSupp_LastPriceUoM','pHelperItemSupp_LastPurchaseQty','pHelperItemSupp_LastBasePrice','pHelperItemSupp_LastPriceWithTax','pHelperItemSupp_LastDiscountFormula','pHelperItemSupp_LastPurchaseValue','pHelperItemSupp_LastRMADocNo','pHelperItemSupp_LastRMADocDate','pHelperItemSupp_LastRMAQty','pHelperItemSupp_LastRMAValue','ItemSuppliersDetails','pHelperSaleOrd_SaleOrdInternalNo','pHelperSaleOrd_SaleOrderDate','pHelperSaleOrd_Priority','pHelperSaleOrd_Customer','pHelperSaleOrd_CustDescription','pHelperSaleOrd_Currency','pHelperSaleOrd_CurrencyDescri','pHelperSaleOrd_FixingDate','pHelperSaleOrd_Fixing','pHelperSaleOrd_ExpectedDeliveryDate','pHelperSaleOrd_ConfirmedDeliveryDate','pHelperSaleOrd_SingleDelivery','pHelperSaleOrd_CompulsoryDeliveryDate','SaleOrdersDetails','pHelperPurchOrd_PurchOrdInternalNo','pHelperPurchOrd_PurchOrderDate','pHelperPurchOrd_Supplier','pHelperPurchOrd_SuppDescription','pHelperPurchOrd_Currency','pHelperPurchOrd_CurrencyDescri','pHelperPurchOrd_FixingDate','pHelperPurchOrd_Fixing','pHelperPurchOrd_ExpectedDeliveryDate','pHelperPurchOrd_ConfirmedDeliveryDate','PurchaseOrdersDetails','pHelperMO_MONo','pHelperMO_MOStatus','pHelperMO_DeliveryDate','pHelperMO_ProductionQty','pHelperMO_UoM','pHelperMO_ProducedQty','pHelperMO_SecondRateQuantity','pHelperMO_ScrapQuantity','DBTLinksTable'],'ItemPriceListsDetails':['PriceList','Disabled','ValidityStartingDate','ValidityEndingDate','PriceListUoM','Price','Qty','DiscountFormula','WithTax','Discounted','AlwaysShow','LastModificationDate','Notes'],'ItemComparableUoMDetails':['ComparableUoM','IsDisabled','NoOfPacksCompUoM','BaseUoMQty','CompUoMQty','GrossWeight','GrossVolume','Packaging','Notes','Factor1','Factor2','Factor3','Factor4','Factor1Description','Factor2Description','Factor3Description','Factor4Description'],'ItemSubstituteItemsDetails':['Substitute','SubsItemDescri','ItemQty','ItemUoM','SubstituteQty','SubsItemUoM','Notes'],'ItemWMSDetails':['Zone','Bin','AvailableQty','SumQtyBaseUoM','SumQtyReserved','SumQtyIncoming'],'ItemCustomersDetails':['ValidityStartingDate','ValidityEndingDate','UoM','Qty','Price','DiscountFormula','WithTax','LastModificationDate','Notes'],'ItemSuppliersDetails':['Operation','ValidityStartingDate','ValidityEndingDate','UoM','Qty','Price','DiscountFormula','WithTax','LastModificationDate','Notes'],'SaleOrdersDetails':['Position','LineType','Item','Description','Variant','UoM','Qty','UnitValue','TaxableAmount','DiscountFormula','TaxCode','Delivered','Invoiced','ExpectedDeliveryDate','ConfirmedDeliveryDate','DrawingAsItemAlias','Lot','PickedQty','NoPrint','NoDN','NoInvoice'],'PurchaseOrdersDetails':['Position','LineType','Item','Description','UoM','Qty','UnitValue','TaxableAmount','DiscountFormula','TaxCode','Paid','ExpectedDeliveryDate','Drawing','Lot','NoPrint','NoDN','NoInvoice'],'DBTLinksTable':['Image','Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_ITEMS_GRAPHIC_NAVIGFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_ITEMS_GRAPHIC_NAVIGComponent, resolver);
    }
} 