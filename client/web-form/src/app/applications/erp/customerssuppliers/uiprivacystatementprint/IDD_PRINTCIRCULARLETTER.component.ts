import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_PRINTCIRCULARLETTERService } from './IDD_PRINTCIRCULARLETTER.service';

@Component({
    selector: 'tb-IDD_PRINTCIRCULARLETTER',
    templateUrl: './IDD_PRINTCIRCULARLETTER.component.html',
    providers: [IDD_PRINTCIRCULARLETTERService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_PRINTCIRCULARLETTERComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_STCIRCULARLETTER_COUNTY_itemSource: any;
public IDC_STCIRCULARLETTER_REGION_itemSource: any;

    constructor(document: IDD_PRINTCIRCULARLETTERService,
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
        this.IDC_STCIRCULARLETTER_COUNTY_itemSource = {
  "name": "ItemSourceXml",
  "namespace": "ItemSource.Framework.TbGes.TbGes.ItemSourceXml",
  "parameter": "DataFile.ERP.Company.County"
}; 
this.IDC_STCIRCULARLETTER_REGION_itemSource = {
  "name": "ItemSourceXml",
  "namespace": "ItemSource.Framework.TbGes.TbGes.ItemSourceXml",
  "parameter": "DataFile.ERP.Company.Region"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['CustSupp','CustSuppAll','CustSuppSel','FromCode','FromCode','ToCode','Category','DescriCategory','Reprint','AllCountry','Country','AllCounty','County','AllCity','City','Reprint','AllCountry','Country','AllCounty','County','AllRegion','Region','AllCity','City','UseTemplate','Template','OnlyExistTpl','UseFreeText','FreeText','PrintAuthSect','DefPrint','Labels','EMail','PrintMail','PostaLite','PrintPostaLite','PLDeliveryType','PLPrintType','ProcessStatus'],'HKLFromCode':['CompanyName','CompanyName'],'HKLToCode':['CompanyName'],'HKLCircularLetterTemplates':['Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_PRINTCIRCULARLETTERFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_PRINTCIRCULARLETTERComponent, resolver);
    }
} 