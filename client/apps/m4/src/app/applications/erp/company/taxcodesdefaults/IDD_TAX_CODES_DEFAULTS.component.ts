import { Component, OnInit, OnDestroy, ComponentFactoryResolver } from '@angular/core';

import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';

import { IDD_TAX_CODES_DEFAULTSService } from './IDD_TAX_CODES_DEFAULTS.service';

@Component({
    selector: 'tb-IDD_TAX_CODES_DEFAULTS',
    templateUrl: './IDD_TAX_CODES_DEFAULTS.component.html',
    providers: [IDD_TAX_CODES_DEFAULTSService, ComponentInfoService]
})
export class IDD_TAX_CODES_DEFAULTSComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_TAX_CODES_DEFAULTSService,
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
export class IDD_TAX_CODES_DEFAULTSFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_TAX_CODES_DEFAULTSComponent, resolver);
    }
} 