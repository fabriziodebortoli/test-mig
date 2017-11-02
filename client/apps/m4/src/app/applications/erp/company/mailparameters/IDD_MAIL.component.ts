import { Component, OnInit, OnDestroy, ComponentFactoryResolver } from '@angular/core';

import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';

import { IDD_MAILService } from './IDD_MAIL.service';

@Component({
    selector: 'tb-IDD_MAIL',
    templateUrl: './IDD_MAIL.component.html',
    providers: [IDD_MAILService, ComponentInfoService]
})
export class IDD_MAILComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_MAILService,
    eventData: EventDataService,
        resolver: ComponentFactoryResolver,
        ciService: ComponentInfoService) {
        super(document, eventData, resolver, ciService);
    }

    ngOnInit() {
        super.ngOnInit();
        
        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['Mail','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'Mail':['l_TEnhMail_Description','FilePath','Namespace','Notes','l_TEnhMail_Disabled']});

    }
    ngOnDestroy() {
        super.ngOnDestroy();
    }

}

@Component({
    template: ''
})
export class IDD_MAILFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_MAILComponent, resolver);
    }
} 