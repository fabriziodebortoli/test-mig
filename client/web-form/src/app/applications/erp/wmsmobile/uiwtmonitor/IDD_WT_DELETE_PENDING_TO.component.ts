import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_WT_DELETE_PENDING_TOService } from './IDD_WT_DELETE_PENDING_TO.service';

@Component({
    selector: 'tb-IDD_WT_DELETE_PENDING_TO',
    templateUrl: './IDD_WT_DELETE_PENDING_TO.component.html',
    providers: [IDD_WT_DELETE_PENDING_TOService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_WT_DELETE_PENDING_TOComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_WT_DELETE_PENDING_TOService,
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
		boService.appendToModelStructure({'global':['Storage_All','Storage_Sel','Storage','SourceZone_All','SourceZone_Sel','SourceZone','DestinationZone_All','DestinationZone_Sel','DestinationZone','Item_All','Item_Sel','FromItem','ToItem','Customer_All','Customer_Sel','Customer','TeamFilter_All','TeamFilter_Sel','TeamFilter','ResourceFilter_All','ResourceFilter_Sel','ResourceFilter','ResourceFilterName','ToConfirmationDate_All','ToConfirmationDate_Sel','FromToConfirmationDate','ToConfirmationDate','WTMonitor','LegendTOClosed','LegendTOTransferred','LegendTOInProgress','LegendTOFailed','LegendTOReadyToBeSync'],'HKLTeamsFilter':['Description'],'WTMonitor':['Selection','ConfirmationDate','AutoID','BmpProperty','SyncStatusBmp','MovementType','OperationType','ChildNumber','ID','ToNumber','Team','ToResource','TeamDescription','WorkerDescription','Item','ItemDescription','Lot','InternalIdNo','CustSupp','CustomerDescription','DocumentNo','Storage','SourceZone','SourceBin','SourceStorageUnit','DestZone','DestBin','DestStorageUnit','UoM','PackingUnit','QtyToMove','QtyMoved','QtyMissing','QtyBroken','SourceSpecialStock','SourceSpecialStockCode','DestSpecialStock','DestSpecialStockCode','MacAddress','ConsignmentPartner','Notes','ErrorMsg','RetryForError','InProcess','InProcessFrom']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_WT_DELETE_PENDING_TOFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_WT_DELETE_PENDING_TOComponent, resolver);
    }
} 