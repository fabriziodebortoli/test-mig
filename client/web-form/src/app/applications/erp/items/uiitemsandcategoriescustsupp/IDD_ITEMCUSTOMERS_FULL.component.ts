import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_ITEMCUSTOMERS_FULLService } from './IDD_ITEMCUSTOMERS_FULL.service';

@Component({
    selector: 'tb-IDD_ITEMCUSTOMERS_FULL',
    templateUrl: './IDD_ITEMCUSTOMERS_FULL.component.html',
    providers: [IDD_ITEMCUSTOMERS_FULLService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_ITEMCUSTOMERS_FULLComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_CUSTSUPITEM_UOM_itemSource: any;
public IDC_CUSTSUPITEM_SUT_SHIPPING_UOM_itemSource: any;
public IDC_CUSTSUPITEM_QTY_UOM_itemSource: any;
public IDC_CUSTSUPITEM_UOM_LASTSALE_itemSource: any;

    constructor(document: IDD_ITEMCUSTOMERS_FULLService,
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
        this.IDC_CUSTSUPITEM_UOM_itemSource = {
  "name": "UnitsOfMeasureItemSuppliersComboBox",
  "namespace": "ERP.Items.Documents.UnitsOfMeasureItemSuppliersComboBox",
  "allowChanges": false,
  "useProductLanguage": false
}; 
this.IDC_CUSTSUPITEM_SUT_SHIPPING_UOM_itemSource = {
  "allowChanges": false,
  "useProductLanguage": false
}; 
this.IDC_CUSTSUPITEM_QTY_UOM_itemSource = {
  "name": "UnitsOfMeasureItemCustomersComboBox",
  "namespace": "ERP.Items.Documents.UnitsOfMeasureItemCustomersComboBox"
}; 
this.IDC_CUSTSUPITEM_UOM_LASTSALE_itemSource = {
  "name": "UnitsOfMeasureItemCustomersComboBox",
  "namespace": "ERP.Items.Documents.UnitsOfMeasureItemCustomersComboBox"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'ItemCustomers':['Item','Disabled','Customer','Notes','Variant','CustomerCode','CustomerDescription','UoMStandardPrice','StandardPrice','StandardPriceWithTax','DiscountFormula','MinOrderQty','AdditionalCharges','ShippingCost','DaysForDelivery','SaleOffset','CustomerBarCode','SUTPreShipping','SUTPreShippingQty','SUTPreShippingUoM','LastSaleDocType','LastSaleDocNo','LastSaleDocDate','LastPriceCurrency','LastPaymentTerm','LastPriceUoM','LastSaleQty','LastPrice','LastPriceWithTax','LastDiscountFormula','LastSaleValue','NetPrice','LastRMADocNo','LastRMADocDate','LastRMAQty','LastRMAValue'],'HKLItems':['Description','BaseUoM','BasePrice','DiscountFormula'],'HKLCustSupp':['CompanyName','Currency'],'global':['CurrencyDescription','DeliveryPlacesDeliveryPlaceDescr','Budget','BracketPriceList','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'DeliveryPlaces':['Address'],'HKLOffsetFixAssets':['Description'],'HKLSUTShipping':['Description'],'Budget':['BudgetYear','BudgetMonth','SaleQty','SaleValue'],'BracketPriceList':['ValidityStartingDate','ValidityEndingDate','UoM','Qty','Price','DiscountFormula','WithTax','LastModificationDate','Notes'],'HKLCurrencies':['Description'],'HKLPymtTerm':['Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_ITEMCUSTOMERS_FULLFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_ITEMCUSTOMERS_FULLComponent, resolver);
    }
} 