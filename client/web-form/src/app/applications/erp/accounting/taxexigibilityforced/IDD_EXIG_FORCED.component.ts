import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_EXIG_FORCEDService } from './IDD_EXIG_FORCED.service';

@Component({
    selector: 'tb-IDD_EXIG_FORCED',
    templateUrl: './IDD_EXIG_FORCED.component.html',
    providers: [IDD_EXIG_FORCEDService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_EXIG_FORCEDComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_EXIG_FORCEDService,
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
		boService.appendToModelStructure({'global':['Sales','Purchases','FromDate','ToDate','AllCustSupp','SelCustSupp','FromCustSupp','ToCustSupp','Template','Reason','GlobalDate','SelectionBody'],'SelectionBody':['l_TEnhTaxExigibilityF_P01','l_TEnhTaxExigibilityF_P02','l_TEnhTaxExigibilityF_P06','l_TEnhTaxExigibilityF_P07','l_TEnhTaxExigibilityF_P08','l_TEnhTaxExigibilityF_P10','l_TEnhTaxExigibilityF_P05','DocNo','LogNo','DocumentDate','PostingDate','l_TEnhTaxExigibilityF_P03','l_TEnhTaxExigibilityF_P04','TaxJournal','l_TEnhTaxExigibilityF_P09']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_EXIG_FORCEDFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_EXIG_FORCEDComponent, resolver);
    }
} 