import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_PRINT_SALEORDService } from './IDD_PRINT_SALEORD.service';

@Component({
    selector: 'tb-IDD_PRINT_SALEORD',
    templateUrl: './IDD_PRINT_SALEORD.component.html',
    providers: [IDD_PRINT_SALEORDService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_PRINT_SALEORDComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_PRINT_SALEORDService,
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
		boService.appendToModelStructure({'global':['StartingDate','EndingDate','AllCustomer','CustomerSel','FromCustomer','ToCustomer','AllInternalNo','InternalNoSel','FromInternalNo','ToInternalNo','AllExternalNo','ExternalNoSel','FromExternalNo','ToExternalNo','AllPriority','PrioritySel','FromPriority','ToPriority','AllSalePeop','SalePeopSel','FromSalesperson','ToSalesperson','NotInvoiced','Invoiced','AllInvoiced','NotDelivered','Delivered','AllDelivered','NotCancelled','Cancelled','AllCancelled','NoPrinted','Printed','AllPrinted','MailNo','MailYes','AllMailed','PostaLiteNo','PostaLiteYes','AllPostaLite','PrintMail','PrintPostaLite','OrderedByCustomer','OrderedByNo','nCurrentElement','GaugeDescription']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_PRINT_SALEORDFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_PRINT_SALEORDComponent, resolver);
    }
} 