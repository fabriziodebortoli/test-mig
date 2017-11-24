import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_DEL_STOCKService } from './IDD_DEL_STOCK.service';

@Component({
    selector: 'tb-IDD_DEL_STOCK',
    templateUrl: './IDD_DEL_STOCK.component.html',
    providers: [IDD_DEL_STOCKService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_DEL_STOCKComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_DEL_STOCKService,
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
        
        		this.bo.appendToModelStructure({'global':['Storage','IntDiff','MintDiff','IntIN','MintIN','IntOUT','MintOUT','bAllItems','bSelectItem','ItemFrom','ItemTo','bAllCreationDate','bSelectCreationDate','CreationDateFrom','CreationDateTo','DBTStocksFromInterim'],'DBTStocksFromInterim':['Selection','Storage','Zone','Bin','StockNumber','Item','Lot','InternalIdNo','UnitOfMeasure','Qty','QtyBaseUoM','CreationDate','SpecialStock','SpecialStockCode','StorageUnit','StorageUnitType','IsMultilevelStorageUnit','QtyReserved','QtyIncoming','LotValidTo','Snapshot','SnapshotCert','SnapshotWorker','SnapshotDate','SnapshotTOId','Weight','Capacity','ConsignmentPartner']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_DEL_STOCKFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_DEL_STOCKComponent, resolver);
    }
} 