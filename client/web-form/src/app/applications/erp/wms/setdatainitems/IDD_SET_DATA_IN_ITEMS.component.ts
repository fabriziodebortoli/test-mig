import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_SET_DATA_IN_ITEMSService } from './IDD_SET_DATA_IN_ITEMS.service';

@Component({
    selector: 'tb-IDD_SET_DATA_IN_ITEMS',
    templateUrl: './IDD_SET_DATA_IN_ITEMS.component.html',
    providers: [IDD_SET_DATA_IN_ITEMSService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_SET_DATA_IN_ITEMSComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_SET_DATA_IN_ITEMSService,
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
        
        		this.bo.appendToModelStructure({'global':['bAllTypes','bSelectType','ItemType','bAllCommodities','bSelectCommodity','ItemCommodityCtg','bAllHomogeneous','bSelectHomogeneous','ItemHomogeneousCtg','bAllProductCtg','bSelectProductCtg','ItemProductCtg','bSetCategory','Category','bSetSectionCategory','SectionCategory','bSetPackingOnPreShipping','SUTPreShipping','SUTPreShippingQty','SUTPreShippingUoM','bSetBarcodeItem','bBarcodeItemFromSales','bBarcodeItemFromItemCode','bSetHazardousMaterial','bSetPrintBarcodeInGR','bSetCrossDocking','bSetUsedInWMSMobile','bHazardousMaterial','bPrintBarcodeInGR','bCrossDocking','bUsedInWMSMobile','bItemsRecalledInTO','DBTItemsCategoriesWMS'],'HKLItemType':['Description'],'HKLCommodityCtg':['Description'],'HKLHomogeneousCtg':['Description'],'HKLProductCtg':['Description'],'HKLCategory':['Description'],'HKLCategoryForPutaway':['Description'],'DBTItemsCategoriesWMS':['Selected','Item'],'HKLItemBE':['Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_SET_DATA_IN_ITEMSFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_SET_DATA_IN_ITEMSComponent, resolver);
    }
} 