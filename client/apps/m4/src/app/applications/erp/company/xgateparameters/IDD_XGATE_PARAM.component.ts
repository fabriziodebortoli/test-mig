﻿import { Component, OnInit, OnDestroy, ComponentFactoryResolver } from '@angular/core';

import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';

import { IDD_XGATE_PARAMService } from './IDD_XGATE_PARAM.service';

@Component({
    selector: 'tb-IDD_XGATE_PARAM',
    templateUrl: './IDD_XGATE_PARAM.component.html',
    providers: [IDD_XGATE_PARAMService, ComponentInfoService]
})
export class IDD_XGATE_PARAMComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_XGATE_PARAMService,
    eventData: EventDataService,
        resolver: ComponentFactoryResolver,
        ciService: ComponentInfoService) {
        super(document, eventData, resolver, ciService);
    }

    ngOnInit() {
        super.ngOnInit();
        
        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg']});

    }
    ngOnDestroy() {
        super.ngOnDestroy();
    }

}

@Component({
    template: ''
})
export class IDD_XGATE_PARAMFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_XGATE_PARAMComponent, resolver);
    }
} 