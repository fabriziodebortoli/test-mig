import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_PURCHASE_ORDERS_TO_RECEIVEService } from './IDD_PURCHASE_ORDERS_TO_RECEIVE.service';

@Component({
    selector: 'tb-IDD_PURCHASE_ORDERS_TO_RECEIVE',
    templateUrl: './IDD_PURCHASE_ORDERS_TO_RECEIVE.component.html',
    providers: [IDD_PURCHASE_ORDERS_TO_RECEIVEService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_PURCHASE_ORDERS_TO_RECEIVEComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_PURCHASE_ORDERS_TO_RECEIVEService,
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
		boService.appendToModelStructure({'global':['PurchaseOrderTOGRLoading'],'PurchaseOrderTOGRLoading':['Sel','DocNo','OrderDate','Item','GRLoading_SupplierCode02','Description','UoM','QtyToRecInGR']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_PURCHASE_ORDERS_TO_RECEIVEFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_PURCHASE_ORDERS_TO_RECEIVEComponent, resolver);
    }
} 