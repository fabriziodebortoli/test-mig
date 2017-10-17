import { Component, OnInit, OnDestroy, ComponentFactoryResolver } from '@angular/core';

import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';

import { IDD_PARAMETERS_CURRENCIESService } from './IDD_PARAMETERS_CURRENCIES.service';

@Component({
    selector: 'tb-IDD_PARAMETERS_CURRENCIES',
    templateUrl: './IDD_PARAMETERS_CURRENCIES.component.html',
    providers: [IDD_PARAMETERS_CURRENCIESService, ComponentInfoService]
})
export class IDD_PARAMETERS_CURRENCIESComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_PARAMETERS_CURRENCIESService,
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
export class IDD_PARAMETERS_CURRENCIESFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_PARAMETERS_CURRENCIESComponent, resolver);
    }
} 