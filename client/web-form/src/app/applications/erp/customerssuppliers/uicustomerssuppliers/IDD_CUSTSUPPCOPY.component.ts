﻿import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_CUSTSUPPCOPYService } from './IDD_CUSTSUPPCOPY.service';

@Component({
    selector: 'tb-IDD_CUSTSUPPCOPY',
    templateUrl: './IDD_CUSTSUPPCOPY.component.html',
    providers: [IDD_CUSTSUPPCOPYService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_CUSTSUPPCOPYComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_CUSTSUPPCOPYService,
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
        
        		this.bo.appendToModelStructure({'global':['CustSuppType','CustSupp','Budget','ItemsData','CopyAcqDate','MaxValues']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_CUSTSUPPCOPYFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_CUSTSUPPCOPYComponent, resolver);
    }
} 