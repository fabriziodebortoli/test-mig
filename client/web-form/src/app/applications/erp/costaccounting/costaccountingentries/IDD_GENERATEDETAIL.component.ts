﻿import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_GENERATEDETAILService } from './IDD_GENERATEDETAIL.service';

@Component({
    selector: 'tb-IDD_GENERATEDETAIL',
    templateUrl: './IDD_GENERATEDETAIL.component.html',
    providers: [IDD_GENERATEDETAILService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_GENERATEDETAILComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_GENERATEDETAILService,
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
        
        		this.bo.appendToModelStructure({'global':['NoDetailDoc','DaysNoDoc','AccrualDateType','YearCommercialDoc','EndMonthDoc']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_GENERATEDETAILFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_GENERATEDETAILComponent, resolver);
    }
} 