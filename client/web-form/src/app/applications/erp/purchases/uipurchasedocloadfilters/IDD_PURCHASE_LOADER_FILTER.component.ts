import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_PURCHASE_LOADER_FILTERService } from './IDD_PURCHASE_LOADER_FILTER.service';

@Component({
    selector: 'tb-IDD_PURCHASE_LOADER_FILTER',
    templateUrl: './IDD_PURCHASE_LOADER_FILTER.component.html',
    providers: [IDD_PURCHASE_LOADER_FILTERService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_PURCHASE_LOADER_FILTERComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_PURCHASE_LOADER_FILTERService,
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
		boService.appendToModelStructure({'global':['OrdNoFilter','PurchaseOrdNoFilter','OrdPosFilter','ExcludeFulffilledLines','ExcludeNotes','QtyToInvoiceAsDelivered','ItemFilter','ShowOnlyItemLine']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_PURCHASE_LOADER_FILTERFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_PURCHASE_LOADER_FILTERComponent, resolver);
    }
} 