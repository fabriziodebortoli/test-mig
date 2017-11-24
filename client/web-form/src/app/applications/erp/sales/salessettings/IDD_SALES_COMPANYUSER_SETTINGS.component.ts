import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_SALES_COMPANYUSER_SETTINGSService } from './IDD_SALES_COMPANYUSER_SETTINGS.service';

@Component({
    selector: 'tb-IDD_SALES_COMPANYUSER_SETTINGS',
    templateUrl: './IDD_SALES_COMPANYUSER_SETTINGS.component.html',
    providers: [IDD_SALES_COMPANYUSER_SETTINGSService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_SALES_COMPANYUSER_SETTINGSComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_SALES_COMPANYUSER_SETTINGSService,
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
        
        		this.bo.appendToModelStructure({'global':['ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_SALES_COMPANYUSER_SETTINGSFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_SALES_COMPANYUSER_SETTINGSComponent, resolver);
    }
} 