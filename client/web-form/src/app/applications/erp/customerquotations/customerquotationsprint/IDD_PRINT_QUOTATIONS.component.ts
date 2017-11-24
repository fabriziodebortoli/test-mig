import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_PRINT_QUOTATIONSService } from './IDD_PRINT_QUOTATIONS.service';

@Component({
    selector: 'tb-IDD_PRINT_QUOTATIONS',
    templateUrl: './IDD_PRINT_QUOTATIONS.component.html',
    providers: [IDD_PRINT_QUOTATIONSService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_PRINT_QUOTATIONSComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_PRINT_QUOTATIONSService,
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
        
        		this.bo.appendToModelStructure({'global':['StartingDate','EndingDate','AllCustomer','CustomerSel','FromCustomer','ToCustomer','AllContact','ContactSel','FromContact','ToContact','AllQuotationNo','QuotationNoSel','FromQuotationNo','ToQuotationNo','NoPrinted','Printed','AllPrinted','MailNo','MailYes','AllMailed','PostaLiteNo','PostaLiteYes','AllPostaLite','PrintMail','PrintPostaLite','OrderedByCustomer','OrderedByNo','nCurrentElement','GaugeDescription']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_PRINT_QUOTATIONSFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_PRINT_QUOTATIONSComponent, resolver);
    }
} 