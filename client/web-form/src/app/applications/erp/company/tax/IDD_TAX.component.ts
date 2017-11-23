import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_TAXService } from './IDD_TAX.service';

@Component({
    selector: 'tb-IDD_TAX',
    templateUrl: './IDD_TAX.component.html',
    providers: [IDD_TAXService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_TAXComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_TAX_CUSTOMERS_LIST_itemSource: any;

    constructor(document: IDD_TAXService,
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
        this.IDC_TAX_CUSTOMERS_LIST_itemSource = {
  "name": "CustListTypeCombo",
  "namespace": "ERP.Accounting_IT.AddOnsMaster.CustomerListEnumCombo"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'TaxCode':['TaxCode','Disabled','Description','Notes','NoChargesDistribution','DistributionPerc','ExemptInvoice','Perc','UndeductiblePerc','FarmerTaxPerc','AdditionalPerc','UseSecondLumpSumRate','LetterForFiscalPrinter','InExportPlafond','InPlafondTurnover','PlafondType','ProRataExempt','InProRataTurnover','TravelAgencyVAT','TravelExtraUE','PurchaseType','BuyerObligedToPayTax','ReverseCharge','PurchaseType','InExportPlafond','NoTaxableAmount','ReverseCharge','D394GoodCode','D394ActivityCode','D394TransactionType','AGOLawCode','AGOTaxCode','OMNIALawCode','OMNIATaxCode','BlackListExempt','BlackListNonTaxable','NotInBlackList','NoTaxableAmount','NoIntrastat','Exempt','NonTaxable','FixedAssets','Gold','Scrap'],'global':['UseFirstLumpSumRate','D394Goods','__Languages','CDTaxYear','TaxDeclaration','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg','DBTLinksTable'],'HKLAGOLawCodes':['Description'],'HKLAGOTaxCodes':['Description'],'HKLOMNIALawCodes':['Description'],'HKLOMNIATaxCodes':['Description'],'@Languages':['__Language','__Description','__Notes','__TextDescri','__TextDescri2'],'TaxCodesLists':['CustListType','SuppListType'],'TaxDeclaration':['Frame','PrintOrder','Line','DataType','ColumnType','AmountType','ActionOnTotal'],'DBTLinksTable':['Image','Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_TAXFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_TAXComponent, resolver);
    }
} 