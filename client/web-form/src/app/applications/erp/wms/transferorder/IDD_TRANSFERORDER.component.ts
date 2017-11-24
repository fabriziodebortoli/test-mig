import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_TRANSFERORDERService } from './IDD_TRANSFERORDER.service';

@Component({
    selector: 'tb-IDD_TRANSFERORDER',
    templateUrl: './IDD_TRANSFERORDER.component.html',
    providers: [IDD_TRANSFERORDERService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_TRANSFERORDERComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_TRANSFERORDERService,
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
        
        		this.bo.appendToModelStructure({'TransferOrder':['ID','TONumber','MovementType','CreationDate','ConfirmationDate','TOStatus','Reason','Team','ToResource','Storage','ConsignmentPartner','Item','Lot','InternalIdNo','BaseUoM','QtyNeeded','UoM','QtyToMove','QtyMoved','SourceZone','SourceBin','SourceStorageUnit','SourceSpecialStock','SourceSpecialStockCode','DestZone','DestBin','DestStorageUnit','DestSpecialStock','DestSpecialStockCode','Notes','CreatedFromInventory','AutoConfirmed'],'HKLWMReason':['Description'],'HKLWMTeams':['Description'],'HKLWMResource':['NameComplete'],'HKLStorage':['Description'],'HKLConsignmentPartner':['CompanyName'],'HKLItem':['Description'],'global':['Detail','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'Detail':['StockNumber','Storage','Zone','Bin','Item','Lot','InternalIdNo','SpecialStock','SpecialStockCode','StorageUnit','StorageUnitType','UnitOfMeasure','Qty','QtyBaseUoM','LotValidTo','ConsignmentPartner']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_TRANSFERORDERFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_TRANSFERORDERComponent, resolver);
    }
} 