﻿import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_VOUCHERSPRINTService } from './IDD_VOUCHERSPRINT.service';

@Component({
    selector: 'tb-IDD_VOUCHERSPRINT',
    templateUrl: './IDD_VOUCHERSPRINT.component.html',
    providers: [IDD_VOUCHERSPRINTService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_VOUCHERSPRINTComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_VOUCHERSPRINTService,
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
		boService.appendToModelStructure({'global':['AllSel1','SlipSel','SlipNo','AllSel','NoSel','FromNo','ToNo','IgnorePrinted','IssueDate','IssueBank','DefPrint','NoticeOfPayments'],'HKLBank':['Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_VOUCHERSPRINTFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_VOUCHERSPRINTComponent, resolver);
    }
} 