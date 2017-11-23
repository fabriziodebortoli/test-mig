import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_BUILD_COMMUNICATIONService } from './IDD_BUILD_COMMUNICATION.service';

@Component({
    selector: 'tb-IDD_BUILD_COMMUNICATION',
    templateUrl: './IDD_BUILD_COMMUNICATION.component.html',
    providers: [IDD_BUILD_COMMUNICATIONService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_BUILD_COMMUNICATIONComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_BUILD_COMMUNICATION_HEIR_CODE_itemSource: any;

    constructor(document: IDD_BUILD_COMMUNICATIONService,
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
        this.IDC_BUILD_COMMUNICATION_HEIR_CODE_itemSource = {
  "name": "ItemSourceXml",
  "namespace": "ItemSource.Framework.TbGes.TbGes.ItemSourceXml",
  "parameter": "DataFile.ERP.Payees.PositionCode"
}; 

        		this.bo.appendToModelStructure({'global':['Year','SEL_prev','RadioSelection','RadioGeneration','RadioPrint','SEL_cust_All','SEL_cust','SEL_custf','SEL_custt','SEL_supp_All','SEL_supp','SEL_suppf','SEL_suppt','SEL_incl','SEL_excl','SEL_cee','SEL_excee','SEL_bl','SEL_onlyIncluded','SEL_onlyExcluded','SEL_company','SEL_person','SEL_notres','nCurrentElement','SelectionBody','SEL_cust_All','SEL_cust','SEL_custf','SEL_custt','SEL_supp_All','SEL_supp','SEL_suppf','SEL_suppt','GenerationStart','nCurrentElement2','Year','GroupedData','FileNameComplete','FileInternet','FileInternetLimit','Entratel','EntratelLimit','Intermediary','CAF','CommitDate','MadeTaxpayer','Heir_FiscalCode','Heir_Code_XML','Heir_From','Heir_To','Sostitutive','Cancellation','ProtocolTel','ProtocolDoc','CompanyGroup'],'SelectionBody':['l_TEnhJournalEntriesTax_P10','l_TEnhJournalEntriesTax_P11','l_TEnhJournalEntriesTax_P01','l_TEnhJournalEntriesTax_P02','l_TEnhJournalEntriesTax_P18','l_TEnhJournalEntriesTax_P17','CustSuppType','l_TEnhJournalEntriesTax_P23','CustSupp','l_TEnhJournalEntriesTax_P16','PostingDate','DocumentDate','DocNo','l_TEnhJournalEntriesTax_P24','l_TEnhJournalEntriesTax_P08','l_TEnhJournalEntriesTax_P09','l_TEnhJournalEntriesTax_P19','l_TEnhJournalEntriesTax_P21','l_TEnhJournalEntriesTax_P20','l_TEnhJournalEntriesTax_P22','l_TEnhJournalEntriesTax_P07','TaxCommunicationGroup','l_TEnhJournalEntriesTax_P05','l_TEnhJournalEntriesTax_P03','l_TEnhJournalEntriesTax_P04','l_TEnhJournalEntriesTax_P06','l_TEnhJournalEntriesTax_P15']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_BUILD_COMMUNICATIONFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_BUILD_COMMUNICATIONComponent, resolver);
    }
} 