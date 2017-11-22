﻿import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_PRODUCTLINEGROUPSService } from './IDD_PRODUCTLINEGROUPS.service';

@Component({
    selector: 'tb-IDD_PRODUCTLINEGROUPS',
    templateUrl: './IDD_PRODUCTLINEGROUPS.component.html',
    providers: [IDD_PRODUCTLINEGROUPSService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_PRODUCTLINEGROUPSComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_PRODUCTLINEGROUPSService,
        eventData: EventDataService,
        resolver: ComponentFactoryResolver,
        private store: Store,
        ciService: ComponentInfoService,
        private changeDetectorRef: ChangeDetectorRef) {
        super(document, eventData, resolver, ciService);
        this.eventData.change.subscribe(() => this.changeDetectorRef.detectChanges());
    }

    ngOnInit() {
        super.ngOnInit();
        
        const boService = this.document as BOService;
		boService.appendToModelStructure({'ProductLineGroups':['GroupCode','Disabled','Description','Notes'],'global':['ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_PRODUCTLINEGROUPSFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_PRODUCTLINEGROUPSComponent, resolver);
    }
} 