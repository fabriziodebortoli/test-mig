import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_BUSINESS_BR_SPED_PARAMETERSService } from './IDD_BUSINESS_BR_SPED_PARAMETERS.service';

@Component({
    selector: 'tb-IDD_BUSINESS_BR_SPED_PARAMETERS',
    templateUrl: './IDD_BUSINESS_BR_SPED_PARAMETERS.component.html',
    providers: [IDD_BUSINESS_BR_SPED_PARAMETERSService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_BUSINESS_BR_SPED_PARAMETERSComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_BUSINESS_BR_SPED_PARAMETERSService,
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
export class IDD_BUSINESS_BR_SPED_PARAMETERSFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_BUSINESS_BR_SPED_PARAMETERSComponent, resolver);
    }
} 