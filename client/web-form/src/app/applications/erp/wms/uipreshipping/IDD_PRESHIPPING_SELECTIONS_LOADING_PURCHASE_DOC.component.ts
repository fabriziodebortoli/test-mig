import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_PRESHIPPING_SELECTIONS_LOADING_PURCHASE_DOCService } from './IDD_PRESHIPPING_SELECTIONS_LOADING_PURCHASE_DOC.service';

@Component({
    selector: 'tb-IDD_PRESHIPPING_SELECTIONS_LOADING_PURCHASE_DOC',
    templateUrl: './IDD_PRESHIPPING_SELECTIONS_LOADING_PURCHASE_DOC.component.html',
    providers: [IDD_PRESHIPPING_SELECTIONS_LOADING_PURCHASE_DOCService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_PRESHIPPING_SELECTIONS_LOADING_PURCHASE_DOCComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_PRESHIPPING_SELECTIONS_LOADING_PURCHASE_DOCService,
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
        
        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['FilterForBillOfLading','FilterForPurchaseInvoices','PurchaseDocNo']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_PRESHIPPING_SELECTIONS_LOADING_PURCHASE_DOCFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_PRESHIPPING_SELECTIONS_LOADING_PURCHASE_DOCComponent, resolver);
    }
} 