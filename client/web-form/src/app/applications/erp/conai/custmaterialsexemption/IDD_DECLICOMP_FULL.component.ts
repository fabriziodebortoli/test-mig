﻿import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_DECLICOMP_FULLService } from './IDD_DECLICOMP_FULL.service';

@Component({
    selector: 'tb-IDD_DECLICOMP_FULL',
    templateUrl: './IDD_DECLICOMP_FULL.component.html',
    providers: [IDD_DECLICOMP_FULLService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_DECLICOMP_FULLComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_DECLICOMP_FULLService,
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
        
        		this.bo.appendToModelStructure({'CustMaterialsExemption':['Customer','Material','NoEntryPosting'],'HKLCustomers':['CompNameComplete'],'global':['CustMaterialsExemptPeriod','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_DECLICOMP_FULLFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_DECLICOMP_FULLComponent, resolver);
    }
} 