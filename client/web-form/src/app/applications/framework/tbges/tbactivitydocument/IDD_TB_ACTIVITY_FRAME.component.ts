﻿import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_TB_ACTIVITY_FRAMEService } from './IDD_TB_ACTIVITY_FRAME.service';

@Component({
    selector: 'tb-IDD_TB_ACTIVITY_FRAME',
    templateUrl: './IDD_TB_ACTIVITY_FRAME.component.html',
    providers: [IDD_TB_ACTIVITY_FRAMEService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_TB_ACTIVITY_FRAMEComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_TB_ACTIVITY_FRAMEService,
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
export class IDD_TB_ACTIVITY_FRAMEFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_TB_ACTIVITY_FRAMEComponent, resolver);
    }
} 