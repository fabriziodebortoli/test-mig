import { Component, OnInit, OnDestroy, ComponentFactoryResolver } from '@angular/core';

import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';

import { IDD_COPYTAXService } from './IDD_COPYTAX.service';

@Component({
    selector: 'tb-IDD_COPYTAX',
    templateUrl: './IDD_COPYTAX.component.html',
    providers: [IDD_COPYTAXService, ComponentInfoService]
})
export class IDD_COPYTAXComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_COPYTAXService,
    eventData: EventDataService,
        resolver: ComponentFactoryResolver,
        ciService: ComponentInfoService) {
        super(document, eventData, resolver, ciService);
    }

    ngOnInit() {
        super.ngOnInit();
        
        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['Tax']});

    }
    ngOnDestroy() {
        super.ngOnDestroy();
    }

}

@Component({
    template: ''
})
export class IDD_COPYTAXFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_COPYTAXComponent, resolver);
    }
} 