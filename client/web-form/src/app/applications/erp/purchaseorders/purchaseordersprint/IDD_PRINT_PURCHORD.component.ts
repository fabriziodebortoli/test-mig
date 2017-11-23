import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_PRINT_PURCHORDService } from './IDD_PRINT_PURCHORD.service';

@Component({
    selector: 'tb-IDD_PRINT_PURCHORD',
    templateUrl: './IDD_PRINT_PURCHORD.component.html',
    providers: [IDD_PRINT_PURCHORDService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_PRINT_PURCHORDComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_PRINT_PURCHORDService,
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
        
        		this.bo.appendToModelStructure({'global':['StartingDate','EndingDate','AllSupp','SuppsSel','SuppStart','SuppEnd','AllInternalNo','InternalNoSel','FromInternalNo','ToInternalNo','AllExternalNo','ExternalNoSel','FromExternalNo','ToExternalNo','NotPayed','Payed','AllPayed','bNotDelivered','bDelivered','bAllDelivered','NotCancelled','Cancelled','AllCancelled','NoPrinted','Printed','AllPrinted','MailNo','MailYes','AllMailed','PostaLiteNo','PostaLiteYes','AllPostaLite','PrintMail','PrintPostaLite','OrderedBySupplier','OrderedByNo','nCurrentElement','GaugeDescription']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_PRINT_PURCHORDFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_PRINT_PURCHORDComponent, resolver);
    }
} 