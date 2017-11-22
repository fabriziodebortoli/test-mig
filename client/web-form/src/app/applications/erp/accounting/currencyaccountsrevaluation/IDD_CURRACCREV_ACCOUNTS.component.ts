﻿import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_CURRACCREV_ACCOUNTSService } from './IDD_CURRACCREV_ACCOUNTS.service';

@Component({
    selector: 'tb-IDD_CURRACCREV_ACCOUNTS',
    templateUrl: './IDD_CURRACCREV_ACCOUNTS.component.html',
    providers: [IDD_CURRACCREV_ACCOUNTSService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_CURRACCREV_ACCOUNTSComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_CURRACCREV_ACCOUNTSService,
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
		boService.appendToModelStructure({'global':['Currency','FixingDate','Fixing','FixingDescri','PostDate','AccrualDate','Nature','bOneJEForCustSupp','ProfitAccount','ProfitAccountDescri','LossAccount','LossAccountDescri']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_CURRACCREV_ACCOUNTSFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_CURRACCREV_ACCOUNTSComponent, resolver);
    }
} 