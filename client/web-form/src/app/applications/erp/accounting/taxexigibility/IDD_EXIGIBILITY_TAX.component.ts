import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_EXIGIBILITY_TAXService } from './IDD_EXIGIBILITY_TAX.service';

@Component({
    selector: 'tb-IDD_EXIGIBILITY_TAX',
    templateUrl: './IDD_EXIGIBILITY_TAX.component.html',
    providers: [IDD_EXIGIBILITY_TAXService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_EXIGIBILITY_TAXComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_EXIGIBILITY_TAXService,
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
        
        		this.bo.appendToModelStructure({'TaxExigibility':['IsManual','TaxJournal','PostingDate','DocumentDate','DocNo','LogNo','SplitPayment','Exigible','ExigibilityDate','TotalAmount','TaxableAmount','TaxAmount','UndeductibleAmount','TaxCode','TEnhTaxExigibility_P6','TEnhTaxExigibility_P1','TEnhTaxExigibility_P4'],'HKLTaxJournals':['Description'],'HKLTAX':['Description'],'HKLCustSupp':['CompNameComplete'],'global':['ID_Link_Tax_Document','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_EXIGIBILITY_TAXFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_EXIGIBILITY_TAXComponent, resolver);
    }
} 