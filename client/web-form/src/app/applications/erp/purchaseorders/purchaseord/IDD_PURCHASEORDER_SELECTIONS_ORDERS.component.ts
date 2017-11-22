import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_PURCHASEORDER_SELECTIONS_ORDERSService } from './IDD_PURCHASEORDER_SELECTIONS_ORDERS.service';

@Component({
    selector: 'tb-IDD_PURCHASEORDER_SELECTIONS_ORDERS',
    templateUrl: './IDD_PURCHASEORDER_SELECTIONS_ORDERS.component.html',
    providers: [IDD_PURCHASEORDER_SELECTIONS_ORDERSService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_PURCHASEORDER_SELECTIONS_ORDERSComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_PURCHASEORDER_SELECTIONS_ORDERSService,
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
		boService.appendToModelStructure({'global':['OrdNoFilter','OrdPosFilter','ExcludeFulffilledLines','ExcludeNotes','ItemFilter','ShowOnlyItemLine']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_PURCHASEORDER_SELECTIONS_ORDERSFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_PURCHASEORDER_SELECTIONS_ORDERSComponent, resolver);
    }
} 