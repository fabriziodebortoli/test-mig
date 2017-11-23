import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_MANUFACT_PARAMETERSService } from './IDD_MANUFACT_PARAMETERS.service';

@Component({
    selector: 'tb-IDD_MANUFACT_PARAMETERS',
    templateUrl: './IDD_MANUFACT_PARAMETERS.component.html',
    providers: [IDD_MANUFACT_PARAMETERSService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_MANUFACT_PARAMETERSComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_MANUFACT_PARAMETERSService,
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
export class IDD_MANUFACT_PARAMETERSFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_MANUFACT_PARAMETERSComponent, resolver);
    }
} 