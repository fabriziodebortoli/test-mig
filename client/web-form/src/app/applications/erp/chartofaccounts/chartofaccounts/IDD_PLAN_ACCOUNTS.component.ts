import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_PLAN_ACCOUNTSService } from './IDD_PLAN_ACCOUNTS.service';

@Component({
    selector: 'tb-IDD_PLAN_ACCOUNTS',
    templateUrl: './IDD_PLAN_ACCOUNTS.component.html',
    providers: [IDD_PLAN_ACCOUNTSService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_PLAN_ACCOUNTSComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_COA_CURRENCY_FOR_BALANCES_itemSource: any;

    constructor(document: IDD_PLAN_ACCOUNTSService,
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
        this.IDC_COA_CURRENCY_FOR_BALANCES_itemSource = {
  "name": "CurrenciesCombo",
  "namespace": "ERP.ChartOfAccounts.Documents.CurrenciesCombo"
}; 

        		this.bo.appendToModelStructure({'ChartOfAccounts':['Account','Disabled','Description','Notes','Ledger','CodeType','PostableInJE','ReportCode','ExternalCode','PostableInCostAcc','CashFlowType','InCurrency','Currency','AGOSubAccount','AGOIntraCode','OMNIASubAccount','OMNIAIntraCode','DebitCreditSign','UoM','DirectCost','FullCost','CostCentersDistribution','JobsDistribution'],'global':['Nature','Nature','NrFiscalYear','BalanceCurrency','DebitActTotal','CreditActTotal','ActualDebitBalance','ActualCreditBalance','DebitForTotal','CreditForTotal','BudgetDebitBalance','BudgetCreditBalance','DebitTotal','CreditTotal','DebitBalance','CreditBalance','Balances','__Languages','CoAReclass','CostAccTemplates','TotalDebitCCTot','TotalCreditCCTot','TotalDebitCCTot','TotalCreditCCTot','TotalDebitJobTot','TotalCreditJobTot','TotalDebitJobTot','TotalCreditJobTot','TotalDebitLineTot','TotalCreditLineTot','TotalDebitLineTot','TotalCreditLineTot','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg','DBTLinksTable'],'HKLCurrency':['Description'],'Balances':['Nature','TEnhCOABalancesMonthYear_P1','Debit','Credit'],'HKLAGOSubAccounts':['Description'],'HKLAGOIntraCodes':['Description'],'HKLOMNIASubAccounts':['Description'],'HKLOMNIAIntraCodes':['Description'],'@Languages':['__Language','__Description','__Notes','__TextDescri','__TextDescri2'],'CoAReclass':['SchemaCode','Code','Description','Account','DebitCreditSign'],'HKLBalReclass':['Description'],'HKLChartOfAccounts':['Description'],'CostAccTemplates':['CostCenter','Job','ProductLine','DebitCreditSign','Perc','Notes'],'HKLDetailCstCenter':['Description'],'HKLDetailJob':['Description'],'HKLDetailProductLine':['Description'],'DBTLinksTable':['Image','Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_PLAN_ACCOUNTSFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_PLAN_ACCOUNTSComponent, resolver);
    }
} 