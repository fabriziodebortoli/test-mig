import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_STINTRAROService } from './IDD_STINTRARO.service';

@Component({
    selector: 'tb-IDD_STINTRARO',
    templateUrl: './IDD_STINTRARO.component.html',
    providers: [IDD_STINTRAROService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_STINTRAROComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_STINTRAROService,
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
        
        		this.bo.appendToModelStructure({'global':['SaleSummary','PurchSummary','Normal','Adjustment','Period','Year','PeriodAdj','YearAdj','ContactPosition','ContactTelephone','ContactName','ContactFAX','ContactSurname','ContactEMail','DelegUse','DelegTaxIdNo','DelegName','DelegStreet','DelegNr','DelegBl','DelegEntr','DelegAp','DelegCounty','DelegZipCode','DelegCity','DBTSummaryDetail'],'DBTSummaryDetail':['l_LineSummaryDescription']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_STINTRAROFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_STINTRAROComponent, resolver);
    }
} 