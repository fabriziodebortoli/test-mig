﻿import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_CUSTOMERCLASSIFICATIONService } from './IDD_CUSTOMERCLASSIFICATION.service';

@Component({
    selector: 'tb-IDD_CUSTOMERCLASSIFICATION',
    templateUrl: './IDD_CUSTOMERCLASSIFICATION.component.html',
    providers: [IDD_CUSTOMERCLASSIFICATIONService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_CUSTOMERCLASSIFICATIONComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_CUSTOMERCLASSIFICATIONService,
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
        
        		this.bo.appendToModelStructure({'CustomerClassification':['CustomerClassification','Description'],'global':['ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_CUSTOMERCLASSIFICATIONFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_CUSTOMERCLASSIFICATIONComponent, resolver);
    }
} 