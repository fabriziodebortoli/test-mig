﻿import { Component, OnInit, OnDestroy, ComponentFactoryResolver } from '@angular/core';

import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';

import { IDD_PROJECT_CODESService } from './IDD_PROJECT_CODES.service';

@Component({
    selector: 'tb-IDD_PROJECT_CODES',
    templateUrl: './IDD_PROJECT_CODES.component.html',
    providers: [IDD_PROJECT_CODESService, ComponentInfoService]
})
export class IDD_PROJECT_CODESComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_PROJECT_CODESService,
    eventData: EventDataService,
        resolver: ComponentFactoryResolver,
        ciService: ComponentInfoService) {
        super(document, eventData, resolver, ciService);
    }

    ngOnInit() {
        super.ngOnInit();
        
        const boService = this.document as BOService;
		boService.appendToModelStructure({'ProjectCodes':['ProjectCode','Disabled','Description','Notes'],'global':['ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg']});

    }
    ngOnDestroy() {
        super.ngOnDestroy();
    }

}

@Component({
    template: ''
})
export class IDD_PROJECT_CODESFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_PROJECT_CODESComponent, resolver);
    }
} 