import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_IBANUPDATEService } from './IDD_IBANUPDATE.service';

@Component({
    selector: 'tb-IDD_IBANUPDATE',
    templateUrl: './IDD_IBANUPDATE.component.html',
    providers: [IDD_IBANUPDATEService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_IBANUPDATEComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_IBANUPDATEService,
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
		boService.appendToModelStructure({'global':['CustSupp','CustSuppAll','CustSuppSel','FromCode','FromCode','ToCode','Category','DescriCategory','UpdateNotEmpty','nCurrentElement','GaugeDescription'],'HKLFromCode':['CompanyName','CompanyName'],'HKLToCode':['CompanyName']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_IBANUPDATEFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_IBANUPDATEComponent, resolver);
    }
} 