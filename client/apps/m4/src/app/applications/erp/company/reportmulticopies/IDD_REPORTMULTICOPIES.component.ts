﻿import { Component, OnInit, OnDestroy, ComponentFactoryResolver } from '@angular/core';

import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';

import { IDD_REPORTMULTICOPIESService } from './IDD_REPORTMULTICOPIES.service';

@Component({
    selector: 'tb-IDD_REPORTMULTICOPIES',
    templateUrl: './IDD_REPORTMULTICOPIES.component.html',
    providers: [IDD_REPORTMULTICOPIESService, ComponentInfoService]
})
export class IDD_REPORTMULTICOPIESComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_REPORTMULTICOPIESService,
    eventData: EventDataService,
        resolver: ComponentFactoryResolver,
        ciService: ComponentInfoService) {
        super(document, eventData, resolver, ciService);
    }

    ngOnInit() {
        super.ngOnInit();
        
        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['Detail','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'Detail':['CopyNo','PrintDescription','ReprintDescription']});

    }
    ngOnDestroy() {
        super.ngOnDestroy();
    }

}

@Component({
    template: ''
})
export class IDD_REPORTMULTICOPIESFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_REPORTMULTICOPIESComponent, resolver);
    }
} 