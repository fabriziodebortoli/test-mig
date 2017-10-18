﻿import { Component, OnInit, OnDestroy, ComponentFactoryResolver } from '@angular/core';

import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';

import { IDD_CONTRACT_CODESService } from './IDD_CONTRACT_CODES.service';

@Component({
    selector: 'tb-IDD_CONTRACT_CODES',
    templateUrl: './IDD_CONTRACT_CODES.component.html',
    providers: [IDD_CONTRACT_CODESService, ComponentInfoService]
})
export class IDD_CONTRACT_CODESComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_CONTRACT_CODESService,
    eventData: EventDataService,
        resolver: ComponentFactoryResolver,
        ciService: ComponentInfoService) {
        super(document, eventData, resolver, ciService);
    }

    ngOnInit() {
        super.ngOnInit();
        
        const boService = this.document as BOService;
		boService.appendToModelStructure({'ContractCodes':['ContractCode','Disabled','Description','Notes'],'global':['ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg']});

    }
    ngOnDestroy() {
        super.ngOnDestroy();
    }

}

@Component({
    template: ''
})
export class IDD_CONTRACT_CODESFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_CONTRACT_CODESComponent, resolver);
    }
} 