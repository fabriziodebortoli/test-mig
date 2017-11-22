import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_ITEMSUPP_FULLService } from './IDD_ITEMSUPP_FULL.service';

@Component({
    selector: 'tb-IDD_ITEMSUPP_FULL',
    templateUrl: './IDD_ITEMSUPP_FULL.component.html',
    providers: [IDD_ITEMSUPP_FULLService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_ITEMSUPP_FULLComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_CUSTSUPITEM_UOM_itemSource: any;
public IDC_CUSTSUPITEM_QTY_UOM_itemSource: any;
public IDC_CUSTSUPITEM_UOM_LASTPURCHASE_itemSource: any;

    constructor(document: IDD_ITEMSUPP_FULLService,
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
        this.IDC_CUSTSUPITEM_UOM_itemSource = {
  "name": "UnitsOfMeasureItemSuppliersComboBox",
  "namespace": "ERP.Items.Documents.UnitsOfMeasureItemSuppliersComboBox"
}; 
this.IDC_CUSTSUPITEM_QTY_UOM_itemSource = {
  "name": "UnitsOfMeasureItemSuppliersComboBox",
  "namespace": "ERP.Items.Documents.UnitsOfMeasureItemSuppliersComboBox"
}; 
this.IDC_CUSTSUPITEM_UOM_LASTPURCHASE_itemSource = {
  "name": "UnitsOfMeasureItemSuppliersComboBox",
  "namespace": "ERP.Items.Documents.UnitsOfMeasureItemSuppliersComboBox"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'ItemSuppliers':['Item','Disabled','Supplier','Currency','Notes','OutsrcPriceListUoM','SupplierCode','SupplierDescription','UoMStandardPrice','StandardPrice','StandardPriceWithTax','DiscountFormula','MinOrderQty','AdditionalCharges','ShippingCost','DaysForDelivery','PurchaseOffset','PostToInspection','SpecificationsForSupplier','LastPurchaseDocType','LastPurchaseDocNo','LastPurchaseDocDate','LastPriceCurrency','LastPaymentTerm','LastPriceUoM','LastPurchaseQty','LastPrice','LastPriceWithTax','LastDiscountFormula','LastPurchaseValue','LastRMADocNo','LastRMADocDate','LastRMAQty','LastRMAValue'],'HKLItems':['Description','BaseUoM','BasePrice','DiscountFormula'],'HKLCustSupp':['CompanyName'],'HKLStdCurrencies':['Description'],'HKLOffsetPurchase':['Description'],'global':['Operations','BracketPriceList','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'Operations':['Operation','FixedCost'],'HKLOperations2':['Description'],'BracketPriceList':['Operation','ValidityStartingDate','ValidityEndingDate','UoM','Qty','Price','DiscountFormula','WithTax','LastModificationDate','Notes'],'HKLCurrencies':['Description'],'HKLPymtTerm':['Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_ITEMSUPP_FULLFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_ITEMSUPP_FULLComponent, resolver);
    }
} 