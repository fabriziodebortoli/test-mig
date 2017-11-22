import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_LOADER_SALE_ORDService } from './IDD_LOADER_SALE_ORD.service';

@Component({
    selector: 'tb-IDD_LOADER_SALE_ORD',
    templateUrl: './IDD_LOADER_SALE_ORD.component.html',
    providers: [IDD_LOADER_SALE_ORDService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_LOADER_SALE_ORDComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_LOADER_SALE_ORDService,
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
		boService.appendToModelStructure({'global':['OrdNoFilter','OrdPosFilter','ExcludeFulffilledLines','ExcludeNotes','QtyToInvoiceAsDelivered','ItemFilter','ShowOnlyItemLine','bUseFilterForStorageForOrderFilter','StoragePhase1ForOrderFilter','StoragePhase2ForOrderFilter','bUseCurrentDNStorages']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_LOADER_SALE_ORDFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_LOADER_SALE_ORDComponent, resolver);
    }
} 