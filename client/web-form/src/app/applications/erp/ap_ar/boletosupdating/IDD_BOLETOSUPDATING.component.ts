import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_BOLETOSUPDATINGService } from './IDD_BOLETOSUPDATING.service';

@Component({
    selector: 'tb-IDD_BOLETOSUPDATING',
    templateUrl: './IDD_BOLETOSUPDATING.component.html',
    providers: [IDD_BOLETOSUPDATINGService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_BOLETOSUPDATINGComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_BOLETOSUPDATINGService,
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
        
        		this.bo.appendToModelStructure({'global':['FromDocNo','CompanyName','IssuingDate','bAlsoBlockedCust','bGenerate','bGenAndPrint','bPrintPreview','bDefPrint','bEMail','bPrintMail'],'HKLBoletos':['IssuerBank','ConditionCode','Customer','Amount','DueDate']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_BOLETOSUPDATINGFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_BOLETOSUPDATINGComponent, resolver);
    }
} 