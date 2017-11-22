﻿import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_REASSIGNSUBIDService } from './IDD_REASSIGNSUBID.service';

@Component({
    selector: 'tb-IDD_REASSIGNSUBID',
    templateUrl: './IDD_REASSIGNSUBID.component.html',
    providers: [IDD_REASSIGNSUBIDService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_REASSIGNSUBIDComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_REASSIGNSUBIDService,
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
		boService.appendToModelStructure({'global':['nCurrentElement','GaugeDescription']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_REASSIGNSUBIDFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_REASSIGNSUBIDComponent, resolver);
    }
} 