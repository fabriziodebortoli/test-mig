﻿import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_BALANCEVALUESRESUMEService } from './IDD_BALANCEVALUESRESUME.service';

@Component({
    selector: 'tb-IDD_BALANCEVALUESRESUME',
    templateUrl: './IDD_BALANCEVALUESRESUME.component.html',
    providers: [IDD_BALANCEVALUESRESUMEService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_BALANCEVALUESRESUMEComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_BALANCEVALUESRESUMEService,
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
        
        		this.bo.appendToModelStructure({'global':['AllCtgs','CtgSel','FromCtg','ToCtg','AllFA','FASel','FromFA','ToFA','ToTDeprRsn','AccDeprRsn','bAccAccumDeprs','bEntriesDelete','Process']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_BALANCEVALUESRESUMEFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_BALANCEVALUESRESUMEComponent, resolver);
    }
} 