import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_SUPPCTG_COMMODITYCTG_FULLService } from './IDD_SUPPCTG_COMMODITYCTG_FULL.service';

@Component({
    selector: 'tb-IDD_SUPPCTG_COMMODITYCTG_FULL',
    templateUrl: './IDD_SUPPCTG_COMMODITYCTG_FULL.component.html',
    providers: [IDD_SUPPCTG_COMMODITYCTG_FULLService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_SUPPCTG_COMMODITYCTG_FULLComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_SUPPCTG_COMMODITYCTG_FULLService,
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
		boService.appendToModelStructure({'CommodityCtgSuppliersCtg':['CommodityCtg','Disabled','SupplierCtg','DiscountFormula','PurchaseOffset','Notes'],'HKLCtgCommodity':['Description'],'HKLSuppliersCtg':['Description'],'HKLPurchaseOffset':['Description'],'global':['ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_SUPPCTG_COMMODITYCTG_FULLFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_SUPPCTG_COMMODITYCTG_FULLComponent, resolver);
    }
} 