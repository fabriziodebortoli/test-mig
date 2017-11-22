import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_FEETEMPLATESService } from './IDD_FEETEMPLATES.service';

@Component({
    selector: 'tb-IDD_FEETEMPLATES',
    templateUrl: './IDD_FEETEMPLATES.component.html',
    providers: [IDD_FEETEMPLATESService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_FEETEMPLATESComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_FEETEMPLATES_HELP_ON_METHODS_itemSource: any;
public IDC_FEETEMPLATES_PERCENTAGE_ENASARCOFIRM_itemSource: any;

    constructor(document: IDD_FEETEMPLATESService,
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
        this.IDC_FEETEMPLATES_HELP_ON_METHODS_itemSource = {
  "name": "MethodsINPSItemSource",
  "namespace": "ERP.Payees.Components.MethodsINPSItemSource"
}; 
this.IDC_FEETEMPLATES_PERCENTAGE_ENASARCOFIRM_itemSource = {
  "name": "PercentualEnasarcoItemSource",
  "namespace": "ERP.Payees.Components.PercentualEnasarcoItemSource"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'FeeTemplates':['FeeTpl','Description','TaxPerc','WithholdingTaxPerc','WithholdingTaxBasePerc','Duty','Form770Frame','DirectorRemuneration','INPSCalculationType','ENASARCOPerc','ENASARCOPercSalesPerson','ENASARCOAssPerc','ENASARCOAssPercSP'],'global':['ComboWT','Detail','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'HKLDutyCodes':['Description'],'HKLINPSParameters':['INPSMethodDescri'],'Detail':['Description','Amount','Tax','WithholdingTax','ENASARCO','INPS','WithholdingTaxExcluded','IsAnAdvanceExpense','CPA']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_FEETEMPLATESFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_FEETEMPLATESComponent, resolver);
    }
} 