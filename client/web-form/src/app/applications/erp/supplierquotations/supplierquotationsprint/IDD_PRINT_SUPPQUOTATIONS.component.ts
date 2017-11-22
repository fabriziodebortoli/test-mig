import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_PRINT_SUPPQUOTATIONSService } from './IDD_PRINT_SUPPQUOTATIONS.service';

@Component({
    selector: 'tb-IDD_PRINT_SUPPQUOTATIONS',
    templateUrl: './IDD_PRINT_SUPPQUOTATIONS.component.html',
    providers: [IDD_PRINT_SUPPQUOTATIONSService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_PRINT_SUPPQUOTATIONSComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_PRINT_SUPPQUOTATIONSService,
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
		boService.appendToModelStructure({'global':['StartingDate','EndingDate','AllSupp','SuppsSel','SuppStart','SuppEnd','AllProspSupp','ProspSuppSel','FromProspectiveSupplier','ToProspectiveSupplier','AllQuotationNo','QuotationNoSel','FromQuotationNo','ToQuotationNo','NotClosed','OnlyClosed','AllClosed','NoPrinted','Printed','AllPrinted','MailNo','MailYes','AllMailed','PostaLiteNo','PostaLiteYes','AllPostaLite','PrintMail','PrintPostaLite','OrderedBySupplier','OrderedByNo','nCurrentElement','GaugeDescription']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_PRINT_SUPPQUOTATIONSFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_PRINT_SUPPQUOTATIONSComponent, resolver);
    }
} 