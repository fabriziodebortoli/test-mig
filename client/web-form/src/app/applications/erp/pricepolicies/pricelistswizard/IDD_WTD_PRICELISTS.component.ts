import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_WTD_PRICELISTSService } from './IDD_WTD_PRICELISTS.service';

@Component({
    selector: 'tb-IDD_WTD_PRICELISTS',
    templateUrl: './IDD_WTD_PRICELISTS.component.html',
    providers: [IDD_WTD_PRICELISTSService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_WTD_PRICELISTSComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_WIZ_RECPRLIST_COMBO_TOPRICELIST_itemSource: any;
public IDC_WIZ_JOBLIS_COMBO_LISTPART_itemSource: any;

    constructor(document: IDD_WTD_PRICELISTSService,
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
        this.IDC_WIZ_RECPRLIST_COMBO_TOPRICELIST_itemSource = {
  "name": "ToPriceListEnumCombo",
  "namespace": "ERP.PricePolicies.BatchDocuments.ListRoundCombo"
}; 
this.IDC_WIZ_JOBLIS_COMBO_LISTPART_itemSource = {
  "name": "FromPriceListEnumCombo",
  "namespace": "ERP.PricePolicies.BatchDocuments.ListPartCombo"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['Generation','Variation','ItemPriceList','TypicalCustItem','CustItemBracket','TypicalSuppItem','ABracketItemSupp','dValidStartDate','CustSuppAll','CustSuppSel','FromCustSupp','ToCustSupp','HFItems_All','HFItems_Range','HFItems_From','HFItems_To','ToPriceList','PriceListDescri','Currency','AlwaysShow','dValidStartDate','dValidityEndingDate','BaseUnitOfMeas','IssueUoM','Discounted','WithTax','ToPriceList','dValidStartDate','OnDepPriceList','FromPriceList','SuppPriceList','Supplier','SamePriceLst','dValidFromDepartureDate','BasePrice','Cost','CostType','FromPriceList','Supplier','PriceSel','DeltaPrice','AbsVariation','Rounding','RoundType','DiscountSel','NewDiscountFormula','BracketManage','QtyBracket','PriceSel','DeltaPrice','Rounding','AbsVariation','RoundType','Currency','DiscountSel','NewDiscountFormula','nCurrentElement','GaugeDescription','ProgressViewer'],'HKLPriceListArrival':['Description'],'HKLPriceListDeparture':['Description','Description'],'HotLinkSupp':['CompanyName','CompanyName'],'QtyBracket':['Qty','VariationPerc','AbsVariation','Discount'],'ProgressViewer':['TEnhProgressViewer_P1','TEnhProgressViewer_P2','TEnhProgressViewer_P3']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_WTD_PRICELISTSFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_WTD_PRICELISTSComponent, resolver);
    }
} 