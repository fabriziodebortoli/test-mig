import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_MRP_SIMULATION_SMALLService } from './IDD_MRP_SIMULATION_SMALL.service';

@Component({
    selector: 'tb-IDD_MRP_SIMULATION_SMALL',
    templateUrl: './IDD_MRP_SIMULATION_SMALL.component.html',
    providers: [IDD_MRP_SIMULATION_SMALLService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_MRP_SIMULATION_SMALLComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_MRP_SIMULATION_SMALLService,
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
		boService.appendToModelStructure({'DBTMRPSimulation':['Simulation','Description','RunDate'],'global':['ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_MRP_SIMULATION_SMALLFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_MRP_SIMULATION_SMALLComponent, resolver);
    }
} 