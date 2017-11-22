import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_MBPROD_REORDService } from './IDD_MBPROD_REORD.service';

@Component({
    selector: 'tb-IDD_MBPROD_REORD',
    templateUrl: './IDD_MBPROD_REORD.component.html',
    providers: [IDD_MBPROD_REORDService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_MBPROD_REORDComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_MBPROD_REORDService,
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
		boService.appendToModelStructure({'global':['bWorkerAll','bWorkerSel','WorkerFrom','WorkerTo','bDateAll','bDateSel','DateFrom','DateTo','bItemAll','bItemSel','sItemFrom','sItemTo','bMinimumStock','bUseStockAvailability','bConsiderReorderLot','bGroupBySupplier','bGroupByDate','DocNo','Selected','IsKanban','WorkerDescription','Item','NatureDesc','AvailQty','ReorderLot','MinimumQty','Qty','UoM','QtyToReord','OrderDate','Supplier','Storage'],'HKLItems':['Description'],'HKLCustSupp':['CompNameComplete'],'HKLStorage':['Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_MBPROD_REORDFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_MBPROD_REORDComponent, resolver);
    }
} 