﻿import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_STORAGEUNIT_VIEWERBUTTONService } from './IDD_STORAGEUNIT_VIEWERBUTTON.service';

@Component({
    selector: 'tb-IDD_STORAGEUNIT_VIEWERBUTTON',
    templateUrl: './IDD_STORAGEUNIT_VIEWERBUTTON.component.html',
    providers: [IDD_STORAGEUNIT_VIEWERBUTTONService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_STORAGEUNIT_VIEWERBUTTONComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_STORAGEUNIT_VIEWERBUTTONService,
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
		boService.appendToModelStructure({});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_STORAGEUNIT_VIEWERBUTTONFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_STORAGEUNIT_VIEWERBUTTONComponent, resolver);
    }
} 