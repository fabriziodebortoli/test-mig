import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_INVENTRY_LOADService } from './IDD_INVENTRY_LOAD.service';

@Component({
    selector: 'tb-IDD_INVENTRY_LOAD',
    templateUrl: './IDD_INVENTRY_LOAD.component.html',
    providers: [IDD_INVENTRY_LOADService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_INVENTRY_LOADComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_INVENTRY_LOADService,
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
		boService.appendToModelStructure({'InvEntriesLoading':['InvRsn','CustSuppType','CustSupp','PreprintedDocNo','DocumentDate','PostingDate','DocNo','Currency'],'HKLInvRsn':['Description'],'HKLCustSupp':['CompanyName'],'HKLCurrencies':['Description'],'global':['InvEntriesDetailLoading'],'InvEntriesDetailLoading':['Selected','Item','Variant','Description','UoM','Qty','UnitValue'],'HKLItems':['Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_INVENTRY_LOADFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_INVENTRY_LOADComponent, resolver);
    }
} 