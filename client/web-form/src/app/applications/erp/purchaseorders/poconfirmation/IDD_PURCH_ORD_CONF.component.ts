import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_PURCH_ORD_CONFService } from './IDD_PURCH_ORD_CONF.service';

@Component({
    selector: 'tb-IDD_PURCH_ORD_CONF',
    templateUrl: './IDD_PURCH_ORD_CONF.component.html',
    providers: [IDD_PURCH_ORD_CONFService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_PURCH_ORD_CONFComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_PURCH_ORD_CONFService,
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
		boService.appendToModelStructure({'global':['Supplier','bAllPO','bSelectedPO','POFrom','POTo','bAllDates','bSelectedDates','PODateFrom','PODateTo','EmptyConfirmNum','EmptyConfirmDate','Job','HFItems_All','HFItems_Range','HFItems_From','HFItems_To','ConfirmationNum','ConfirmationDate','POConfirmations'],'HKLSupplier':['CompanyName'],'POConfirmations':['POConfirma_Selected','InternalPurchOrdNo','Position','PurchOrdDate','ConfirmationNum','Item','UoM','Supplier','Job','Qty','DeliveredQty','QtyToConfirm','Notes','ExtendedNotes','ExpectedDeliveryDate','ConfirmedDeliveryDate','PreviousConfirmedDeliveryDate'],'HKLItemBody':['Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_PURCH_ORD_CONFFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_PURCH_ORD_CONFComponent, resolver);
    }
} 