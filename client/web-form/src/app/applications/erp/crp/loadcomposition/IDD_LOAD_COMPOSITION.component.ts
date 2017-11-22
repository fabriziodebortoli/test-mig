﻿import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_LOAD_COMPOSITIONService } from './IDD_LOAD_COMPOSITION.service';

@Component({
    selector: 'tb-IDD_LOAD_COMPOSITION',
    templateUrl: './IDD_LOAD_COMPOSITION.component.html',
    providers: [IDD_LOAD_COMPOSITIONService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_LOAD_COMPOSITIONComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_LOAD_COMPOSITIONService,
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
		boService.appendToModelStructure({'global':['DBTLoadComposition'],'DBTLoadComposition':['IsSelected','MONo','RtgStep','MachineAlternate','WC','SimStartDate','OldSimStartDate','OldSimEndDate','HoursInTheDay','Setup','IncidenceOnTotalAmount','RtgStep','Alternate','AltRtgStep','Operation','DescriptOperation','Customer','DescriptCust','Job','Product','DescriptItm','UoM','QtyToProduce','StatusDecod','ProcessTimeSIM','EstimatedSetupTime']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_LOAD_COMPOSITIONFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_LOAD_COMPOSITIONComponent, resolver);
    }
} 