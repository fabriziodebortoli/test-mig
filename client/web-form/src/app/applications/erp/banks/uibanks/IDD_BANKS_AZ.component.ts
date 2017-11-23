import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_BANKS_AZService } from './IDD_BANKS_AZ.service';

@Component({
    selector: 'tb-IDD_BANKS_AZ',
    templateUrl: './IDD_BANKS_AZ.component.html',
    providers: [IDD_BANKS_AZService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_BANKS_AZComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_BANKS_STATUS_itemSource: any;
public IDC_BANKS_COUNTY_itemSource: any;
public IDC_BE_BILLSCAS_PRESENTATION_itemSource: any;

    constructor(document: IDD_BANKS_AZService,
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
        this.IDC_BANKS_STATUS_itemSource = {
  "name": "ItemSourceXml",
  "namespace": "ItemSource.Framework.TbGes.TbGes.ItemSourceXml",
  "parameter": "DataFile.ERP.Company.State"
}; 
this.IDC_BANKS_COUNTY_itemSource = {
  "name": "ItemSourceXml",
  "namespace": "ItemSource.Framework.TbGes.TbGes.ItemSourceXml",
  "parameter": "DataFile.ERP.Company.County"
}; 
this.IDC_BE_BILLSCAS_PRESENTATION_itemSource = {
  "name": "BillsPresentationEnumCombo",
  "namespace": "ERP.Banks.Documents.BillsPresentationEnumCombo"
}; 

        		this.bo.appendToModelStructure({'Banks':['Bank','Disabled','Description','IsForeign','ISOCountryCode','ABI','ABIPrefix','CAB','CABPrefix','Swift','SIACode','CBICode','ZIPCode','Address','StreetNo','Address2','District','FederalState','City','Country','Counter','Agency','Branch','Address','Address2','City','ZIPCode','County','Country','Counter','Agency','Branch','Telephone1','Telephone2','Telex','Fax','ContactPerson','EMail','Internet','Identifier','Signature','Notes','BankDays','SenderCode','SenderReference','BillsAndPaymentsFolder','BillsAndPaymentsExtension','UseValueDate','CashOrderCBICode','CashOrderResultRequest','UseISO20022','Account','ChargesAccount','DebitChargesSeparately','PreferredCA','FactoringCode','FactoringCurrency'],'global':['People','BanksCAs','BillsCAs','FactoringCAs','FactoringCustomers','FactoringPymtTerms','BanksConditions','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'People':['TitleCode','ExternalCode','LastName','Name','Telephone1','Telephone2','Telex','Fax','EMail','SkypeID','WorkingPosition','Notes'],'HKLAccountBank':['Description'],'HKLChargesAccountBank':['Description'],'BanksCAs':['CA','CIN','CACheck','IBANIsManual','IBAN','InternalNumber','Account','Disabled','Currency','PymtCash','Notes'],'HKLCAAccount':['Description'],'BillsCAs':['CA','IBAN','PostalNumber','InternalNumber','Blocked','Presentation','Account','Currency','MaxCreditLimit','Presented','BorrowingRate','Notes','DebitCollCharges'],'HKLBillsCA':['Description'],'FactoringCAs':['CA','FactoringType','FactoringAdvance','BorrowingRate','Account','Notes'],'HKLCAAccountFactoring':['Description'],'FactoringCustomers':['Customer','ExternalCode'],'HKLCustomer':['CompanyName'],'FactoringPymtTerms':['Payment','ExternalCode'],'HKLPymtTerm':['Description'],'BanksConditions':['ConditionCode','CompanyBankID','Convenio','CA','Carteira','CarteiraOnFile','RegisteredCollection','IssueSendByBank','MinRange','MaxRange','LastNumber','DiscountRate','InterestRate','PenalityRate','ProtestDays','PaymentPlace','Approved','FileLayout','LastFileID','ReportNamespace']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_BANKS_AZFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_BANKS_AZComponent, resolver);
    }
} 