import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_BR_VEHICLEService } from './IDD_BR_VEHICLE.service';

@Component({
    selector: 'tb-IDD_BR_VEHICLE',
    templateUrl: './IDD_BR_VEHICLE.component.html',
    providers: [IDD_BR_VEHICLEService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_BR_VEHICLEComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_BR_VEHICLEService,
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
        
        		this.bo.appendToModelStructure({'DBTBRVehicle':['Code','Description','Property','VehicleType','FuelType','RNTC','LicensePlate','RegFederalState','RegYear','EngineSize','Color','FrameNumber','TareWeightKg','CapacityKg','CapacityM3'],'global':['ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_BR_VEHICLEFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_BR_VEHICLEComponent, resolver);
    }
} 