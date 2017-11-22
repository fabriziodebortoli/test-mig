import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_CHANGE_ZONE_BIN_LOTService } from './IDD_CHANGE_ZONE_BIN_LOT.service';

@Component({
    selector: 'tb-IDD_CHANGE_ZONE_BIN_LOT',
    templateUrl: './IDD_CHANGE_ZONE_BIN_LOT.component.html',
    providers: [IDD_CHANGE_ZONE_BIN_LOTService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_CHANGE_ZONE_BIN_LOTComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_CHANGE_ZONE_BIN_LOTService,
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
		boService.appendToModelStructure({'global':['LineOriginalSourceZone','LineNewSourceZone','LineOriginalDestinationZone','LineNewDestinationZone','LineOriginalSourceBin','LineNewSourceBin','LineOriginalDestinationBin','LineNewDestinationBin','LineOriginalTeamName','LineNewTeam','LineNewTeamDescription','LineOriginalFullName','LineNewResource','LineNewFullName','LineOldItem','LineNewItem','LineOriginalLot','LineNewLot','LineOldSourceStorageUnit','LineNewSourceStorageUnit','LineOriginalInternalIdNo','LineNewInternalIdNo']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_CHANGE_ZONE_BIN_LOTFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_CHANGE_ZONE_BIN_LOTComponent, resolver);
    }
} 