import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_FEESService } from './IDD_FEES.service';

@Component({
    selector: 'tb-IDD_FEES',
    templateUrl: './IDD_FEES.component.html',
    providers: [IDD_FEESService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_FEESComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_FEES_CALCULATION_itemSource: any;
public IDC_FEES_PERCENTAGE_itemSource: any;
public IDC_FEES_DATA_MODALITARA_itemSource: any;
public IDC_FEES_DATA_MODALITAINPS_itemSource: any;
public IDC_FEES_DATA_MODALITAENASARCO_itemSource: any;
public IDC_FEES_DATA_LETTER770_itemSource: any;

    constructor(document: IDD_FEESService,
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
        this.IDC_FEES_CALCULATION_itemSource = {
  "name": "MethodsINPSItemSource",
  "namespace": "ERP.Payees.Components.MethodsINPSItemSource"
}; 
this.IDC_FEES_PERCENTAGE_itemSource = {
  "name": "PercentualEnasarcoItemSource",
  "namespace": "ERP.Payees.Components.PercentualEnasarcoItemSource"
}; 
this.IDC_FEES_DATA_MODALITARA_itemSource = {
  "name": "MethodsPaymentCombo",
  "namespace": "ERP.Payees.Documents.MethodsPaymentCombo"
}; 
this.IDC_FEES_DATA_MODALITAINPS_itemSource = {
  "name": "MethodsPaymentCombo",
  "namespace": "ERP.Payees.Documents.MethodsPaymentCombo"
}; 
this.IDC_FEES_DATA_MODALITAENASARCO_itemSource = {
  "name": "MethodsPaymentEnasarcoCombo",
  "namespace": "ERP.Payees.Documents.MethodsPaymentEnasarcoCombo"
}; 
this.IDC_FEES_DATA_LETTER770_itemSource = {
  "name": "ItemSourceXml",
  "namespace": "ItemSource.Framework.TbGes.TbGes.ItemSourceXml",
  "parameter": "DataFile.ERP.Payees.ReasonCU"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'Fees':['CustSupp','DocumentDate','DocNo','Template','ProForma','LogNo','TaxPerc','Tax','l_RefJENo','l_AccTpl','PaymentDate','WithholdingTaxDate','Description','Duty','WithholdingTaxBasePerc','WithholdingTaxPerc','WithholdingTax','WithholdingTax','WithholdingTaxPaid','WithholdingTaxSuspended','INPSCalculationType','INPS','INPSEmployees','INPSPaid','AccrualDate','l_SalespersonData','ENASARCOPerc','ENASARCO','ENASARCOAssPerc','ENASARCOAss','ENASARCOPercSalesPerson','ENASARCOSalesperson','ENASARCOAssPercSP','ENASARCOAssSP','WithholdingTaxPymtDate','WithholdingTaxPymMethod','ProgrNumber','Series','PymtNumber','PymtNumber','INPSPymtDate','INPSPymtMethod','ENASARCOPymtDate','ENASARCOPymtMethod','Currency','FixingDate','Fixing','Payment','PayableAmount','Paid','FeePaid','WithholdingTaxTransfer','INPSTransfer','ENASARCOTransfer','Form770Frame','Form770Letter','StandardLetter','DirectorRemuneration','WithholdingTaxAccrualYear'],'HKLSupplierCode':['CompNameComplete'],'HKLFeeTemplates':['Description'],'HKLRSNRit':['Description'],'HKLMethodsINPS':['INPSMethodDescri'],'global':['PercentageCombo','Detail','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'Detail':['Line','Description','Amount','Tax','WithholdingTax','ENASARCO','INPS','WithholdingTaxExcluded','IsAnAdvanceExpense','CPA'],'HKLCurrenciesCurrObj':['Description'],'HKLPymtTerm':['Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_FEESFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_FEESComponent, resolver);
    }
} 