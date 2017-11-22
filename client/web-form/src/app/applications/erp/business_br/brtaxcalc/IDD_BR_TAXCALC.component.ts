import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_BR_TAXCALCService } from './IDD_BR_TAXCALC.service';

@Component({
    selector: 'tb-IDD_BR_TAXCALC',
    templateUrl: './IDD_BR_TAXCALC.component.html',
    providers: [IDD_BR_TAXCALCService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_BR_TAXCALCComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_BR_TAXCALC_TAXTYPE_itemSource: any;

    constructor(document: IDD_BR_TAXCALCService,
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
        this.IDC_BR_TAXCALC_TAXTYPE_itemSource = {
  "allowChanges": false,
  "name": "BRTaxCalcEnumCombo",
  "namespace": "ERP.Business_BR.Components.BRTaxCalcEnumCombo",
  "useProductLanguage": false
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'DBTBRTaxCalc':['TaxCalcCode','Description','TaxType','ValidityStartingDate','ValidityEndingDate','TaxCode','TaxRateCode','ReducTaxRateCode','TaxFormulaCode'],'HKLBRTaxCode':['Description'],'HKLBRTaxRateCode':['Description'],'HKLBRReducTaxRateCode':['Description'],'HKLBRTaxFormulaCode':['Description'],'global':['ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_BR_TAXCALCFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_BR_TAXCALCComponent, resolver);
    }
} 