import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_PICKING_AND_PACKINGService } from './IDD_PICKING_AND_PACKING.service';

@Component({
    selector: 'tb-IDD_PICKING_AND_PACKING',
    templateUrl: './IDD_PICKING_AND_PACKING.component.html',
    providers: [IDD_PICKING_AND_PACKINGService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_PICKING_AND_PACKINGComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_PICKING_AND_PACKINGService,
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
        
        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['To_All','To_Sel','FromTo','ToTo','ToCreationDate_All','ToCreationDate_Sel','FromToCreationDate','ToCreationDate','Storage_All','Storage_Sel','Storage','SourceZone_All','SourceZone_Sel','SourceZone','DestinationZone_All','DestinationZone_Sel','DestinationZone','Item_All','Item_Sel','FromItem','ToItem','TeamFilter_All','TeamFilter_Sel','TeamFilter','ResourceFilter_All','ResourceFilter_Sel','ResourceFilter','bWithoutInterimTO','Reason_All','Reason_Sel','Reason','bConsignmentPartner_All','bConsignmentPartner_Sel','ConsignmentPartner','PackingUnit','Selection','BmpProperty','ID','TONumber','CreationDate','Team','ToResource','Item','Reason','Lot','InternalIdNo','Storage','SourceZone','SourceBin','SourceStorageUnit','DestZone','DestBin','DestStorageUnit','UoM','QtyToMove','QtyMoved','QtyMoved','PackingUnit','PackingUnit','QtyMissing','QtyBroken','IsToReturn','InsertForDifference','CreatedFromInventory','ConfirmationDate','QtyNeeded','SourceSpecialStock','SourceSpecialStockCode','DestSpecialStock','DestSpecialStockCode','MovementType','ConsignmentPartner','Notes','StatusCreated','StatusProgress','StatusConfirmed','StatusCancelled'],'HKLTeamsFilter':['Description'],'HKLWorkersFilter':['NameComplete'],'HKLTeamsBody':['Description'],'HKLWorkersBody':['NameComplete'],'HKLItemBE':['Description'],'HKLReason':['Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_PICKING_AND_PACKINGFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_PICKING_AND_PACKINGComponent, resolver);
    }
} 