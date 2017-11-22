import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_BR_DELETING_NFEService } from './IDD_BR_DELETING_NFE.service';

@Component({
    selector: 'tb-IDD_BR_DELETING_NFE',
    templateUrl: './IDD_BR_DELETING_NFE.component.html',
    providers: [IDD_BR_DELETING_NFEService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_BR_DELETING_NFEComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_BR_DELETING_NFEService,
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
		boService.appendToModelStructure({'global':['bNFForCust','bNFForSupp','Model','Series','bAllDate','bDateSel','StartingDate','EndingDate','bAllNum','bNumSel','StartingNum','EndingNum','CustSuppType','bAllCustSupp','bCustSuppSel','StartingCustSupp','EndingCustSupp','Reason','Detail'],'Detail':['TEnhDeleti_bSelected','TEnhDeleti_CustSupp','TEnhDeleti_CustSuppName','TEnhDeleti_DocNo','TEnhDeleti_DocDate','TEnhDeleti_ChNFe','TEnhDeleti_nProt','TEnhDeleti_ElabResult']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_BR_DELETING_NFEFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_BR_DELETING_NFEComponent, resolver);
    }
} 