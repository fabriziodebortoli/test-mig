﻿import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_GRPRESHIPCROSSREFERENCE_VIEWERBUTTONService } from './IDD_GRPRESHIPCROSSREFERENCE_VIEWERBUTTON.service';

@Component({
    selector: 'tb-IDD_GRPRESHIPCROSSREFERENCE_VIEWERBUTTON',
    templateUrl: './IDD_GRPRESHIPCROSSREFERENCE_VIEWERBUTTON.component.html',
    providers: [IDD_GRPRESHIPCROSSREFERENCE_VIEWERBUTTONService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_GRPRESHIPCROSSREFERENCE_VIEWERBUTTONComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_GRPRESHIPCROSSREFERENCE_VIEWERBUTTONService,
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
export class IDD_GRPRESHIPCROSSREFERENCE_VIEWERBUTTONFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_GRPRESHIPCROSSREFERENCE_VIEWERBUTTONComponent, resolver);
    }
} 