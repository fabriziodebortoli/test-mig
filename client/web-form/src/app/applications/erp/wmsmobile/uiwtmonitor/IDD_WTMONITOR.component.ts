import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_WTMONITORService } from './IDD_WTMONITOR.service';

@Component({
    selector: 'tb-IDD_WTMONITOR',
    templateUrl: './IDD_WTMONITOR.component.html',
    providers: [IDD_WTMONITORService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_WTMONITORComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_WTMONITORService,
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
		boService.appendToModelStructure({'global':['Storage_All','Storage_Sel','Storage','SourceZone_All','SourceZone_Sel','SourceZone','DestinationZone_All','DestinationZone_Sel','DestinationZone','Item_All','Item_Sel','FromItem','ToItem','To_All','To_Sel','FromTo','ToTo','SyncStatus_All','SyncStatus_Sel','SyncStatus','OperationType_All','OperationType_Sel','OperationType','Customer_All','Customer_Sel','Customer','bExcludeTransferred','bExcludeClosed','TeamFilter_All','TeamFilter_Sel','TeamFilter','ResourceFilter_All','ResourceFilter_Sel','ResourceFilter','ResourceFilterName','Team','Resource','WTMonitor','LegendTOClosed','LegendTOTransferred','LegendTOInProgress','LegendTOFailed','LegendTOReadyToBeSync'],'HKLTeamsFilter':['Description'],'HKLTeams':['Description'],'HKLWorkersResult':['NameComplete'],'WTMonitor':['Selection','ConfirmationDate','AutoID','BmpProperty','SyncStatusBmp','MovementType','OperationType','ChildNumber','ID','ToNumber','Team','ToResource','TeamDescription','WorkerDescription','Item','ItemDescription','Lot','InternalIdNo','CustSupp','CustomerDescription','DocumentNo','Storage','SourceZone','SourceBin','SourceStorageUnit','DestZone','DestBin','DestStorageUnit','UoM','PackingUnit','QtyToMove','QtyMoved','QtyMissing','QtyBroken','SourceSpecialStock','SourceSpecialStockCode','DestSpecialStock','DestSpecialStockCode','MacAddress','ConsignmentPartner','Notes','ErrorMsg','RetryForError','InProcess','InProcessFrom']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_WTMONITORFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_WTMONITORComponent, resolver);
    }
} 