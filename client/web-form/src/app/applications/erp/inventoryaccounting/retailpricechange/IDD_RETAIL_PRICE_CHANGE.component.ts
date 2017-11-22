import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_RETAIL_PRICE_CHANGEService } from './IDD_RETAIL_PRICE_CHANGE.service';

@Component({
    selector: 'tb-IDD_RETAIL_PRICE_CHANGE',
    templateUrl: './IDD_RETAIL_PRICE_CHANGE.component.html',
    providers: [IDD_RETAIL_PRICE_CHANGEService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_RETAIL_PRICE_CHANGEComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_RETAIL_PRICE_CHANGEService,
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
        
        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['SelectedStorage','HFItems_All','HFItems_Range','HFItems_From','HFItems_To','DeltaPrice','Rounding','RoundType','ApplyCurrentPrice','ApplyLastCost','AccTemplate','AccReason','PostingDate','AssignPrices','TotNetPriceDiff','GrandTotDiff','TotVATDiff'],'HKLRetailStorages':['Description'],'HKLAccTpl':['Description'],'HKLAccRsn':['Description'],'AssignPrices':['Selected','Item','ItemDescription','Price','LastCost','PriceWithTax','NewPrice','BookInvQty','NetPriceDiff','VATDiff','TotDiff']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_RETAIL_PRICE_CHANGEFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_RETAIL_PRICE_CHANGEComponent, resolver);
    }
} 