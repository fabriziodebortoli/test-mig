import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_DELETE_SALEORDService } from './IDD_DELETE_SALEORD.service';

@Component({
    selector: 'tb-IDD_DELETE_SALEORD',
    templateUrl: './IDD_DELETE_SALEORD.component.html',
    providers: [IDD_DELETE_SALEORDService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_DELETE_SALEORDComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_DELETE_SALEORDService,
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
		boService.appendToModelStructure({'global':['StartingDate','EndingDate','AllCustomer','CustomerSel','FromCustomer','ToCustomer','AllInternalNo','InternalNoSel','FromInternalNo','ToInternalNo','AllExternalNo','ExternalNoSel','FromExternalNo','ToExternalNo','AllPayed','NotPayed','Payed','AllAccounting','NotInAccounting','InAccounting','AllCancelled','NotCancelled','Cancelled','AllPrinted','NoPrinted','Printed','nCurrentElement','GaugeDescription']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_DELETE_SALEORDFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_DELETE_SALEORDComponent, resolver);
    }
} 