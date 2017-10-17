import { Component, OnInit, OnDestroy, ComponentFactoryResolver } from '@angular/core';

import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';

import { IDD_CURRENCIESService } from './IDD_CURRENCIES.service';

@Component({
    selector: 'tb-IDD_CURRENCIES',
    templateUrl: './IDD_CURRENCIES.component.html',
    providers: [IDD_CURRENCIESService, ComponentInfoService]
})
export class IDD_CURRENCIESComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_DECURRENCIES_CURRENCY_itemSource: any;

    constructor(document: IDD_CURRENCIESService,
    eventData: EventDataService,
        resolver: ComponentFactoryResolver,
        ciService: ComponentInfoService) {
        super(document, eventData, resolver, ciService);
    }

    ngOnInit() {
        super.ngOnInit();
        this.IDC_DECURRENCIES_CURRENCY_itemSource = {
  "name": "ItemSourceXml",
  "namespace": "ItemSource.Framework.TbGes.TbGes.ItemSourceXml",
  "parameter": "DataFile.ERP.Currencies.Currencies"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'Currencies':['Currency','Disabled','Description','Symbol','InternationalCode','IsEUCurrency','AmountRoundingType','AmountRoundingDigit','TaxAmountRoundingType','TaxAmountRoundingDigit','NoOfDecimals','Notes'],'global':['Fixing','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'Fixing':['ReferredCurrency','FixingDate','Fixing','SaleFixing','PurchaseFixing','Notes'],'HKLCurrencies':['Description']});

    }
    ngOnDestroy() {
        super.ngOnDestroy();
    }

}

@Component({
    template: ''
})
export class IDD_CURRENCIESFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_CURRENCIESComponent, resolver);
    }
} 