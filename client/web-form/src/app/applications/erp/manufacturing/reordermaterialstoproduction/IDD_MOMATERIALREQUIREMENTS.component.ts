import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_MOMATERIALREQUIREMENTSService } from './IDD_MOMATERIALREQUIREMENTS.service';

@Component({
    selector: 'tb-IDD_MOMATERIALREQUIREMENTS',
    templateUrl: './IDD_MOMATERIALREQUIREMENTS.component.html',
    providers: [IDD_MOMATERIALREQUIREMENTSService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_MOMATERIALREQUIREMENTSComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_MOMATERIALREQUIREMENTSService,
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
		boService.appendToModelStructure({'global':['NrSchema','bUseRoutingForCalculationLifePeriodMO','bUseLeadTimeForCalculationLifePeriodMO','MOMaterialRequirements'],'HKLSchema':['Description'],'MOMaterialRequirements':['PurchaseOrdGeneration','CodeType','Component','ComponentVariant','ComponentDescription','UoM','ProductionPlanQty','InducedRequirements','ReservedQty','MOOnHandStorageQty','IncomingQty','MinimumStock','ReorderProposed','QtyToOrder','Availaibility','ReorderingLotSize','DeliveryDate']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_MOMATERIALREQUIREMENTSFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_MOMATERIALREQUIREMENTSComponent, resolver);
    }
} 