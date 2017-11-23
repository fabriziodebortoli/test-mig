import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_DATA_PLAFOND_TAXService } from './IDD_DATA_PLAFOND_TAX.service';

@Component({
    selector: 'tb-IDD_DATA_PLAFOND_TAX',
    templateUrl: './IDD_DATA_PLAFOND_TAX.component.html',
    providers: [IDD_DATA_PLAFOND_TAXService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_DATA_PLAFOND_TAXComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_DATA_PLAFOND_TAXService,
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
		boService.appendToModelStructure({'TaxPlafondData':['BalanceYear','BalanceMonth','Inside','Importing','EUPurchases','ForecastInside','ForecastImporting','ForecastEUPurchases'],'global':['Currency','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_DATA_PLAFOND_TAXFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_DATA_PLAFOND_TAXComponent, resolver);
    }
} 