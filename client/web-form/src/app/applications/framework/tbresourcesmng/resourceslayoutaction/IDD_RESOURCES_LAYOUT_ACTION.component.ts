﻿import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_RESOURCES_LAYOUT_ACTIONService } from './IDD_RESOURCES_LAYOUT_ACTION.service';

@Component({
    selector: 'tb-IDD_RESOURCES_LAYOUT_ACTION',
    templateUrl: './IDD_RESOURCES_LAYOUT_ACTION.component.html',
    providers: [IDD_RESOURCES_LAYOUT_ACTIONService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_RESOURCES_LAYOUT_ACTIONComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_RESOURCES_LAYOUT_ACTIONService,
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
        
        		this.bo.appendToModelStructure({'global':['sActionMessage']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_RESOURCES_LAYOUT_ACTIONFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_RESOURCES_LAYOUT_ACTIONComponent, resolver);
    }
} 