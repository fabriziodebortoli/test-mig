﻿import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_STPRIVACYSTATEMENTService } from './IDD_STPRIVACYSTATEMENT.service';

@Component({
    selector: 'tb-IDD_STPRIVACYSTATEMENT',
    templateUrl: './IDD_STPRIVACYSTATEMENT.component.html',
    providers: [IDD_STPRIVACYSTATEMENTService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_STPRIVACYSTATEMENTComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_STPRIVACYSTATEMENTService,
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
        
        		this.bo.appendToModelStructure({'global':['CustSupp','CustSuppAll','CustSuppSel','FromCode','FromCode','ToCode','Category','DescriCategory','Reprint','DefPrint','Labels','EMail','PrintMail','PostaLite','PrintPostaLite','PLDeliveryType','PLPrintType','ProcessStatus'],'HKLFromCode':['CompanyName','CompanyName'],'HKLToCode':['CompanyName']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_STPRIVACYSTATEMENTFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_STPRIVACYSTATEMENTComponent, resolver);
    }
} 