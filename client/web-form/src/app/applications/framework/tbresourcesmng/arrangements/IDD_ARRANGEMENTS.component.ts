﻿import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_ARRANGEMENTSService } from './IDD_ARRANGEMENTS.service';

@Component({
    selector: 'tb-IDD_ARRANGEMENTS',
    templateUrl: './IDD_ARRANGEMENTS.component.html',
    providers: [IDD_ARRANGEMENTSService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_ARRANGEMENTSComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_ARRANGEMENTSService,
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
        
        		this.bo.appendToModelStructure({'Arrangements':['Arrangement','Description','ArrangementLevel','WorkingHours','BasicPay','TotalPay','Notes'],'global':['ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_ARRANGEMENTSFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_ARRANGEMENTSComponent, resolver);
    }
} 