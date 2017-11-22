import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_ITM_LISService } from './IDD_ITM_LIS.service';

@Component({
    selector: 'tb-IDD_ITM_LIS',
    templateUrl: './IDD_ITM_LIS.component.html',
    providers: [IDD_ITM_LISService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_ITM_LISComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_ITM_LIS_UOM_PRICELIST_itemSource: any;

    constructor(document: IDD_ITM_LISService,
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
        this.IDC_ITM_LIS_UOM_PRICELIST_itemSource = {
  "name": "UnitsOfMeasureFromItmLisDocComboBox",
  "namespace": "ERP.PricePolicies.Documents.UnitsOfMeasureFromItmLisDocComboBox"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'ItemsPriceLists':['PriceList','Disabled','Item','Price','DiscountFormula','PriceListUoM','Qty','WithTax','Discounted','ValidityStartingDate','ValidityEndingDate','LastModificationDate'],'HKLPriceLists':['Description'],'HKLItems':['Description'],'global':['ItemBasePrice','ItemDiscountFormula','ItemBaseUoM','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_ITM_LISFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_ITM_LISComponent, resolver);
    }
} 