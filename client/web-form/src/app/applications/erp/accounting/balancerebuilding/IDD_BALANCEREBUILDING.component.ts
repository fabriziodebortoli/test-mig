import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_BALANCEREBUILDINGService } from './IDD_BALANCEREBUILDING.service';

@Component({
    selector: 'tb-IDD_BALANCEREBUILDING',
    templateUrl: './IDD_BALANCEREBUILDING.component.html',
    providers: [IDD_BALANCEREBUILDINGService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_BALANCEREBUILDINGComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_BALANCEREBUILDINGService,
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
		boService.appendToModelStructure({'global':['ForecastProcess','ActualProcess','BothProcess','ActualProcess','BothProcess','Accounts','AccountOpen','bAnyAccount','CoAsFromMonth','FromYearAcc','bOneAccount','FromYearAcc','bOneAccount','CoAsToMonth','ToYearAcc','AccountCode','ToYearAcc','AccountCode','CustSupp','CustSuppOpen','bAnyCustSupp','CustSuppFromMonth','FromYearCustSupp','bOneCustSupp','FromYearCustSupp','bOneCustSupp','CustSuppToMonth','ToYearCustSupp','CustSuppType','CustSuppCode','ToYearCustSupp','CustSuppType','CustSuppCode','CustSuppType','CustSuppCode','TaxJourn','TaxJournalFromMonth','PurchTaxJ','TaxJournalToMonth','SaleTaxJ','RetailTaxJ','RetailToBeDTaxJ','TaxPlafond','TaxPlafondFromMonth','TaxPlafondToMonth','JEType','Suspended','nCurrentElement','GaugeDescription'],'HKLAccount':['Description'],'HKLCustSupp':['CompNameComplete']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_BALANCEREBUILDINGFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_BALANCEREBUILDINGComponent, resolver);
    }
} 