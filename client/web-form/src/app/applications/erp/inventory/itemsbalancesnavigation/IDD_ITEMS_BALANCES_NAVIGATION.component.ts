import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_ITEMS_BALANCES_NAVIGATIONService } from './IDD_ITEMS_BALANCES_NAVIGATION.service';

@Component({
    selector: 'tb-IDD_ITEMS_BALANCES_NAVIGATION',
    templateUrl: './IDD_ITEMS_BALANCES_NAVIGATION.component.html',
    providers: [IDD_ITEMS_BALANCES_NAVIGATIONService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_ITEMS_BALANCES_NAVIGATIONComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_ITEMS_BALANCES_NAVIGATIONService,
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
        
        		this.bo.appendToModelStructure({'global':['Item','Storage','SpecificatorType','Specificator','bAllVariants','bVariantSel','Variant','bAllLots','bLotSel','Lot','bAlsoOutOfStock','Item','BaseUoM','LastCost','AverageCost','WeightedAverage','InitialBookInv','FinalBookInventory','InitialBookInvValue','BookInvValue','InitialOnHand','FinalOnHand','OrderedPurchOrd','OrderedToProd','ReservedSaleOrd','AllocatedQty','ReservedByProd','Availability','PurchasesQty','PurchasesValue','SalesQty','SalesValue','ScrapQty','ScrapValue','ReceivedQty','ReceivedValue','IssuedQty','IssuedValue','CIGValue','InitialReturnedQty','ReturnedQty','InitialForRepairing','ForRepairing','InitialSampleGoods','SampleGoods','InitialSampling','Sampling','InitialBailment','Bailment','PickedQty','ProducedQty','ProducedValue','InitialUsedByProduction','UsedInProduction','InitialUsedInProductionValue','UsedInProductionValue','PickingValue','InitialSubcontracting','Subcontracting','InitialForSubcontracting','ForSubcontracting','CustomDescription1','InitialCustomQty1','InitialCustomValue1','CustomQty1','CustomValue1','CustomDescription2','InitialCustomQty2','InitialCustomValue2','CustomQty2','CustomValue2','CustomDescription3','InitialCustomQty3','InitialCustomValue3','CustomQty3','CustomValue3','CustomDescription4','InitialCustomQty4','InitialCustomValue4','CustomQty4','CustomValue4','CustomDescription5','InitialCustomQty5','InitialCustomValue5','CustomQty5','CustomValue5','LegendFiscalYearImg','LegendStorageImg','LegendSpecificatorImg','LegendLotImg'],'HKLItems':['Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_ITEMS_BALANCES_NAVIGATIONFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_ITEMS_BALANCES_NAVIGATIONComponent, resolver);
    }
} 