import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_BRROMANEIO_MAINTENANCEService } from './IDD_BRROMANEIO_MAINTENANCE.service';

@Component({
    selector: 'tb-IDD_BRROMANEIO_MAINTENANCE',
    templateUrl: './IDD_BRROMANEIO_MAINTENANCE.component.html',
    providers: [IDD_BRROMANEIO_MAINTENANCEService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_BRROMANEIO_MAINTENANCEComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_BRROMANEIO_MAINTENANCEService,
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
        
        		this.bo.appendToModelStructure({'DBTBRRomaneioMaintenance':['RomaneioNo','RomaneioDate','Driver','Status','DepartureDate','DepartureKm','ArrivalDate','ArrivalKm','Tractor','TractorLicensePlate','TractorFuelType','Trailer','trailerLicensePlate','trailerFuelType'],'HKLWorkers':['NameComplete'],'global':['ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_BRROMANEIO_MAINTENANCEFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_BRROMANEIO_MAINTENANCEComponent, resolver);
    }
} 