import { Component, OnInit, OnDestroy, ComponentFactoryResolver } from '@angular/core';

import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';

import { IDD_ACTIVITYCODESService } from './IDD_ACTIVITYCODES.service';

@Component({
    selector: 'tb-IDD_ACTIVITYCODES',
    templateUrl: './IDD_ACTIVITYCODES.component.html',
    providers: [IDD_ACTIVITYCODESService, ComponentInfoService]
})
export class IDD_ACTIVITYCODESComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_ACTIVITYCODESService,
    eventData: EventDataService,
        resolver: ComponentFactoryResolver,
        ciService: ComponentInfoService) {
        super(document, eventData, resolver, ciService);
    }

    ngOnInit() {
        super.ngOnInit();
        
        const boService = this.document as BOService;
		boService.appendToModelStructure({'ActivityCode':['ActivityCode','Description'],'global':['ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg']});

    }
    ngOnDestroy() {
        super.ngOnDestroy();
    }

}

@Component({
    template: ''
})
export class IDD_ACTIVITYCODESFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_ACTIVITYCODESComponent, resolver);
    }
} 