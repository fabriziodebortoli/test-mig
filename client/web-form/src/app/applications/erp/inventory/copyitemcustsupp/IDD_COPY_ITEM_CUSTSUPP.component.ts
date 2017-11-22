import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_COPY_ITEM_CUSTSUPPService } from './IDD_COPY_ITEM_CUSTSUPP.service';

@Component({
    selector: 'tb-IDD_COPY_ITEM_CUSTSUPP',
    templateUrl: './IDD_COPY_ITEM_CUSTSUPP.component.html',
    providers: [IDD_COPY_ITEM_CUSTSUPPService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_COPY_ITEM_CUSTSUPPComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_COPY_ITEM_CUSTSUPPService,
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
		boService.appendToModelStructure({'global':['bCustCode','bSuppCode','bItemCode','bItemType','bCommCtg','bCustCtg','bSuppCtg','InputDataFrom','InputDataOn','bCustItem','bCustMainData','bCustStandardData','bLastSaleReturn','bCustGridQtyBrackets','bCustItemType','bCustCommCtg','bCustNotes','bCustCommCtgData','bCommCtgLastSaleReturn','bSuppItem','bSuppMainData','bSuppStandardData','bLastPurchaseReturn','bSuppGridQtyBrackets','bSuppItemType','bSuppCommCtg','bSuppNotes','bSuppCommCtgData','bCommCtgLastPurchaseReturn','bCustItemType','bSuppItemType','bCustCommCtg','bCustNotes','bCustCommCtgData','bCommCtgLastSaleReturn','bCustCtgCommCtg','bSuppCommCtg','bSuppNotes','bSuppCommCtgData','bCommCtgLastPurchaseReturn','bSuppCtgCommCtg','bCustCtgCommCtg','bSuppCtgCommCtg','bNoUpdateExistingData','bUpdateExistingData','nCurrentElement']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_COPY_ITEM_CUSTSUPPFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_COPY_ITEM_CUSTSUPPComponent, resolver);
    }
} 