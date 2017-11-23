﻿import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_DELETE_QUALITYINSPECTIONService } from './IDD_DELETE_QUALITYINSPECTION.service';

@Component({
    selector: 'tb-IDD_DELETE_QUALITYINSPECTION',
    templateUrl: './IDD_DELETE_QUALITYINSPECTION.component.html',
    providers: [IDD_DELETE_QUALITYINSPECTIONService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_DELETE_QUALITYINSPECTIONComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_DELETE_QUALITYINSPECTIONService,
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
		boService.appendToModelStructure({'global':['InspectionOrder','InspectionNotes','StartingDate','EndingDate','AllSupp','SuppsSel','SuppStart','SuppEnd','AllNo','NoSel','FromNo','ToNo','AllCancelled','NotCancelled','Cancelled','AllPrinted','NoPrinted','Printed','nCurrentElement','GaugeDescription']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_DELETE_QUALITYINSPECTIONFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_DELETE_QUALITYINSPECTIONComponent, resolver);
    }
} 