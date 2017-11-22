import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_ITEMS_BALANCES_DETAILSService } from './IDD_ITEMS_BALANCES_DETAILS.service';

@Component({
    selector: 'tb-IDD_ITEMS_BALANCES_DETAILS',
    templateUrl: './IDD_ITEMS_BALANCES_DETAILS.component.html',
    providers: [IDD_ITEMS_BALANCES_DETAILSService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_ITEMS_BALANCES_DETAILSComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_ITEMS_BALANCES_DETAILSService,
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
        
        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['ItemsBalancesDetails'],'ItemsBalancesDetails':['Storage','SpecificatorType','Specificator','Lot','Variant','InitialOnHand','FinalOnHand','InitialBookInv','BookInv','InitialBookInvValue','BookInvValue','ReservedSaleOrd','OrderedPurchOrd','LastCost','PurchasesQty','PurchasesValue','ProducedQty','ProducedValue','SalesQty','SalesValue','CIGValue','ScrapQty','ScrapsValue','ReceivedQty','ReceivedValue','IssuedQty','IssuedValue','InitialUsedByProduction','UsedByProduction','InitialUsedInProductionValue','UsedInProductionValue','PickingValue','PickedQty','CustomQty1','CustomValue1','InitialCustomQty1','InitialCustomValue1','CustomQty2','CustomValue2','InitialCustomQty2','InitialCustomValue2','CustomQty3','CustomValue3','InitialCustomQty3','InitialCustomValue3','CustomQty4','CustomValue4','InitialCustomQty4','InitialCustomValue4','CustomQty5','CustomValue5','InitialCustomQty5','InitialCustomValue5']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_ITEMS_BALANCES_DETAILSFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_ITEMS_BALANCES_DETAILSComponent, resolver);
    }
} 