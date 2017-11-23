﻿import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_ACCBOOKATTACH_COPYService } from './IDD_ACCBOOKATTACH_COPY.service';

@Component({
    selector: 'tb-IDD_ACCBOOKATTACH_COPY',
    templateUrl: './IDD_ACCBOOKATTACH_COPY.component.html',
    providers: [IDD_ACCBOOKATTACH_COPYService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_ACCBOOKATTACH_COPYComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_ACCBOOKATTACH_COPYService,
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
        
        		this.bo.appendToModelStructure({'global':['FiscalYearCopy','AttachCode']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_ACCBOOKATTACH_COPYFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_ACCBOOKATTACH_COPYComponent, resolver);
    }
} 