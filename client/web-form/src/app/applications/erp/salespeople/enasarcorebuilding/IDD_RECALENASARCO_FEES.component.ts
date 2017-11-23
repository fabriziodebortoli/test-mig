﻿import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_RECALENASARCO_FEESService } from './IDD_RECALENASARCO_FEES.service';

@Component({
    selector: 'tb-IDD_RECALENASARCO_FEES',
    templateUrl: './IDD_RECALENASARCO_FEES.component.html',
    providers: [IDD_RECALENASARCO_FEESService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_RECALENASARCO_FEESComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_RECALENASARCO_FEESService,
        eventData: EventDataService,
        resolver: ComponentFactoryResolver,
        private store: Store,
        ciService: ComponentInfoService,
        changeDetectorRef: ChangeDetectorRef) {
		super(document, eventData, ciService, changeDetectorRef, resolver);
        this.eventData.change.subscribe(() => this.changeDetectorRef.detectChanges());
    }

    ngOnInit() {
        super.ngOnInit();
        
        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['ClearPaid','bAnyFee','bOnlyPeriod','FromMonth','ToMonth','BigStateProc','LittleStateProc','AccrualFeeDate','MovementCounter']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_RECALENASARCO_FEESFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_RECALENASARCO_FEESComponent, resolver);
    }
} 