﻿import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_CRP_CONFIRMATIONService } from './IDD_CRP_CONFIRMATION.service';

@Component({
    selector: 'tb-IDD_CRP_CONFIRMATION',
    templateUrl: './IDD_CRP_CONFIRMATION.component.html',
    providers: [IDD_CRP_CONFIRMATIONService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_CRP_CONFIRMATIONComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_CRP_CONFIRMATIONService,
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
		boService.appendToModelStructure({'global':['bMoMrp','CRPMOSelection'],'CRPMOSelection':['TMO_Selection','MONo','BOM','Variant','Job','Customer','InternalOrdNo','DeliveryDate'],'HKLBOM':['Description'],'HKLJob':['Description'],'HKLCustomer':['CompanyName']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_CRP_CONFIRMATIONFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_CRP_CONFIRMATIONComponent, resolver);
    }
} 