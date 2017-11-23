﻿import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_OMNIA_PARAMETERSService } from './IDD_OMNIA_PARAMETERS.service';

@Component({
    selector: 'tb-IDD_OMNIA_PARAMETERS',
    templateUrl: './IDD_OMNIA_PARAMETERS.component.html',
    providers: [IDD_OMNIA_PARAMETERSService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_OMNIA_PARAMETERSComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_OMNIA_PARAMETERSService,
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
export class IDD_OMNIA_PARAMETERSFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_OMNIA_PARAMETERSComponent, resolver);
    }
} 