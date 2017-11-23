import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_PRINTINTENTANNULMENTService } from './IDD_PRINTINTENTANNULMENT.service';

@Component({
    selector: 'tb-IDD_PRINTINTENTANNULMENT',
    templateUrl: './IDD_PRINTINTENTANNULMENT.component.html',
    providers: [IDD_PRINTINTENTANNULMENTService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_PRINTINTENTANNULMENTComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_PRINTINTENTANNULMENTService,
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
        
        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['Year','LogNo','Reprint','CustSuppAll','CustSuppSel','FromCode','CustSuppSel','FromCode','FromCode','ToCode','TaxCode','ISOCode','FromDate','ToDate','DefPrint','Labels','EMail','PrintMail','PostaLite','PrintPostaLite','PLDeliveryType','PLPrintType','ProcessStatus'],'HKLFromCode':['CompanyName','CompanyName','CompanyName'],'HKLToCode':['CompanyName']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_PRINTINTENTANNULMENTFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_PRINTINTENTANNULMENTComponent, resolver);
    }
} 