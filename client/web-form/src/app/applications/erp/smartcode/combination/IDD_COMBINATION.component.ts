﻿import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_COMBINATIONService } from './IDD_COMBINATION.service';

@Component({
    selector: 'tb-IDD_COMBINATION',
    templateUrl: './IDD_COMBINATION.component.html',
    providers: [IDD_COMBINATIONService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_COMBINATIONComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_COMBINATIONService,
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
		boService.appendToModelStructure({'SegmentsComb':['Segment','Combination'],'HKLSegments':['Description'],'HKLSegmentsComb':['Description'],'global':['SegmentsCombState','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'SegmentsCombState':['ISOCountryCode','Description','Currency','Language','Price','Notes'],'HKLISOCountryCode':['Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_COMBINATIONFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_COMBINATIONComponent, resolver);
    }
} 