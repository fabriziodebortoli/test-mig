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
        changeDetectorRef: ChangeDetectorRef) {
		super(document, eventData, ciService, changeDetectorRef, resolver);
        this.subscriptions.push(this.eventData.change.subscribe(() => changeDetectorRef.detectChanges()));
    }

    ngOnInit() {
        super.ngOnInit();
        
        		this.bo.appendToModelStructure({'global':['OrdNoFilter','OrdPosFilter','ExcludeFulffilledLines','ExcludeNotes','ItemFilter','ShowOnlyItemLine']});

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