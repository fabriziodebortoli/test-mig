import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_SDDMANDATEPRINTService } from './IDD_SDDMANDATEPRINT.service';

@Component({
    selector: 'tb-IDD_SDDMANDATEPRINT',
    templateUrl: './IDD_SDDMANDATEPRINT.component.html',
    providers: [IDD_SDDMANDATEPRINTService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_SDDMANDATEPRINTComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_SDDMANDATEPRINTService,
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
		boService.appendToModelStructure({'global':['AllCustomers','CustomersSel','FromCode','FromCode','ToCode','AllSDD','DraftSDD','FromMandateCode','ToMandateCode','Reprint','DefPrint','Labels','EMail','PrintMail','PostaLite','PrintPostaLite','PLDeliveryType','PLPrintType','nCurrentElement','GaugeDescription'],'HKLFromCode':['CompanyName','CompanyName'],'HKLToCode':['CompanyName']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_SDDMANDATEPRINTFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_SDDMANDATEPRINTComponent, resolver);
    }
} 