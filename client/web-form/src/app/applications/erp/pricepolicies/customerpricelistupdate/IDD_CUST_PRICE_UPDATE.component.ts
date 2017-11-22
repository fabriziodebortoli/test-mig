import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_CUST_PRICE_UPDATEService } from './IDD_CUST_PRICE_UPDATE.service';

@Component({
    selector: 'tb-IDD_CUST_PRICE_UPDATE',
    templateUrl: './IDD_CUST_PRICE_UPDATE.component.html',
    providers: [IDD_CUST_PRICE_UPDATEService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_CUST_PRICE_UPDATEComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_CUST_PRICE_UPDATEService,
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
		boService.appendToModelStructure({'global':['AllCustomer','CustomerSel','FromCustomer','ToCustomer','OldPriceList','NewPriceList','nCurrentElement','GaugeDescription']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_CUST_PRICE_UPDATEFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_CUST_PRICE_UPDATEComponent, resolver);
    }
} 