import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_DELETE_QUOTATIONSService } from './IDD_DELETE_QUOTATIONS.service';

@Component({
    selector: 'tb-IDD_DELETE_QUOTATIONS',
    templateUrl: './IDD_DELETE_QUOTATIONS.component.html',
    providers: [IDD_DELETE_QUOTATIONSService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_DELETE_QUOTATIONSComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_DELETE_QUOTATIONSService,
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
		boService.appendToModelStructure({'global':['StartingDate','EndingDate','AllCustomer','CustomerSel','FromCustomer','ToCustomer','AllContact','ContactSel','FromContact','ToContact','AllQuotationNo','QuotationNoSel','FromQuotationNo','ToQuotationNo','AllPrinted','NoPrinted','Printed','nCurrentElement','GaugeDescription']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_DELETE_QUOTATIONSFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_DELETE_QUOTATIONSComponent, resolver);
    }
} 