import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_SOGRAPHICNAVService } from './IDD_SOGRAPHICNAV.service';

@Component({
    selector: 'tb-IDD_SOGRAPHICNAV',
    templateUrl: './IDD_SOGRAPHICNAV.component.html',
    providers: [IDD_SOGRAPHICNAVService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_SOGRAPHICNAVComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_SOGRAPHICNAVService,
        eventData: EventDataService,
        resolver: ComponentFactoryResolver,
        private store: Store,
        ciService: ComponentInfoService,
        changeDetectorRef: ChangeDetectorRef) {
		super(document, eventData, ciService, changeDetectorRef, resolver);
        this.eventData.change.subscribe(() => this.changeDetectorRef.detectChanges());
    }

    ngOnInit() {
        super.ngOnInit();
        
        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['bAllCustomer','bSelectCustomer','Customer','bAllNumbers','bSelectNumber','SONumberFrom','SONumberTo','bAllDates','bSelectDate','DateFrom','DateTo','DBTNodeDetail','LegendIsertedStatus','LegendAllocatedStatus','LegendCancelledStatus','LegendDeliveryStatus','LegendInvoicedStatus','LegendInPreshStatus','LegendBlockedStatus'],'DBTNodeDetail':['l_FieldValue','l_FieldName']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_SOGRAPHICNAVFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_SOGRAPHICNAVComponent, resolver);
    }
} 