﻿import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_WC_FAMILYService } from './IDD_WC_FAMILY.service';

@Component({
    selector: 'tb-IDD_WC_FAMILY',
    templateUrl: './IDD_WC_FAMILY.component.html',
    providers: [IDD_WC_FAMILYService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_WC_FAMILYComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_WC_FAMILYService,
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
		boService.appendToModelStructure({'WCFamily':['WCFamily','Description','Notes'],'global':['WCFamilyDetails','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'WCFamilyDetails':['WC','Priority','Preferential','Notes'],'HKLWCBE':['Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_WC_FAMILYFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_WC_FAMILYComponent, resolver);
    }
} 