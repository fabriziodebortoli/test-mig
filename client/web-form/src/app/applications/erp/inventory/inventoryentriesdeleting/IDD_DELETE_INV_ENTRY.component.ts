import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_DELETE_INV_ENTRYService } from './IDD_DELETE_INV_ENTRY.service';

@Component({
    selector: 'tb-IDD_DELETE_INV_ENTRY',
    templateUrl: './IDD_DELETE_INV_ENTRY.component.html',
    providers: [IDD_DELETE_INV_ENTRYService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_DELETE_INV_ENTRYComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_DELETE_INV_ENTRYService,
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
		boService.appendToModelStructure({'global':['pEraserStartingDate','pEraserEndingDate','pEraserFromPostingDate','pEraserToPostingDate','pEraserReason','pEraserStubBook','pEraserStoragePhase1','pEraserStoragePhase2','pEraserUseCustSuppType','pEraserCustSuppType','pEraserFromCustSupp','pEraserToCustSupp','HFItems_All','HFItems_Range','HFItems_From','HFItems_To','pEraserOnlySaleDoc','pEraserOnlyPurchaseDoc','bDeleteLinkedDocument','pEraserSaleDocResetFlagPosted','pEraserPurchaseDocResetFlagPosted','pEraserDoMessageForNotLinkedEntries','nCurrentElement','GaugeDescription']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_DELETE_INV_ENTRYFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_DELETE_INV_ENTRYComponent, resolver);
    }
} 