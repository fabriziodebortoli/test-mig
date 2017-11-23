import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_CORE_COMPANYUSER_SETTINGSService } from './IDD_CORE_COMPANYUSER_SETTINGS.service';

@Component({
    selector: 'tb-IDD_CORE_COMPANYUSER_SETTINGS',
    templateUrl: './IDD_CORE_COMPANYUSER_SETTINGS.component.html',
    providers: [IDD_CORE_COMPANYUSER_SETTINGSService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_CORE_COMPANYUSER_SETTINGSComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_CORE_COMPANYUSER_SETTINGSService,
        eventData: EventDataService,
        resolver: ComponentFactoryResolver,
        private store: Store,
        ciService: ComponentInfoService,
        changeDetectorRef: ChangeDetectorRef) {
		super(document, eventData, ciService, changeDetectorRef, resolver);
        this.subscriptions.push(this.eventData.change.subscribe(() => changeDetectorRef.detectChanges()));
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
export class IDD_CORE_COMPANYUSER_SETTINGSFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_CORE_COMPANYUSER_SETTINGSComponent, resolver);
    }
} 