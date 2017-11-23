﻿import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_IMPORT_SCHEMA_RECLASSIFIED_COAService } from './IDD_IMPORT_SCHEMA_RECLASSIFIED_COA.service';

@Component({
    selector: 'tb-IDD_IMPORT_SCHEMA_RECLASSIFIED_COA',
    templateUrl: './IDD_IMPORT_SCHEMA_RECLASSIFIED_COA.component.html',
    providers: [IDD_IMPORT_SCHEMA_RECLASSIFIED_COAService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_IMPORT_SCHEMA_RECLASSIFIED_COAComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_IMPORT_SCHEMA_RECLASSIFIED_COAService,
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
        
        		this.bo.appendToModelStructure({'global':['Schema']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_IMPORT_SCHEMA_RECLASSIFIED_COAFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_IMPORT_SCHEMA_RECLASSIFIED_COAComponent, resolver);
    }
} 