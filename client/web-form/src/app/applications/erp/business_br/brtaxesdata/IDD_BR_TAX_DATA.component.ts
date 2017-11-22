import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_BR_TAX_DATAService } from './IDD_BR_TAX_DATA.service';

@Component({
    selector: 'tb-IDD_BR_TAX_DATA',
    templateUrl: './IDD_BR_TAX_DATA.component.html',
    providers: [IDD_BR_TAX_DATAService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_BR_TAX_DATAComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_BR_TAX_DATAService,
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
		boService.appendToModelStructure({'global':['BRTaxesDataProductValue_VProd_ForTest','BRTaxesDataShipCharges_ForTest','BRTaxesDataAdditionalCharges_ForTest','BRTaxesDataInsuranceCharges_ForTest','BRTaxesDataDiscountValue_VDesc_ForTest','BRTaxesDataItem','BRTaxesDataItemDescription','BRTaxesDataProductAddInfo','BRTaxesDataNCM','BRTaxesDataCFOP','BRTaxesDataCFOPDescription','BRTaxesDataCFOPCompany','BRTaxesDataCFOPCompanyDescription','BRTaxesDataProductValue_VProd','BRTaxesDataDiscountValue_VDesc','BRTaxesDataIITaxable','BRTaxesDataIIPerc','BRTaxesDataIIValue','BRTaxesDataPISTaxable','BRTaxesDataPISPerc','BRTaxesDataPISValue','BRTaxesDataPISFiscalValue','BRTaxesDataPISCode','BRTaxesDataPISCodeDescri','BRTaxesDataPISCodeCompany','BRTaxesDataPISCodeCompanyDescri','BRTaxesDataPISType','BRTaxesDataCOFINSTaxable','BRTaxesDataCOFINSPerc','BRTaxesDataCOFINSValue','BRTaxesDataCOFINSFiscalValue','BRTaxesDataCOFINSCode','BRTaxesDataCOFINSCodeDescri','BRTaxesDataCOFINSCodeCompany','BRTaxesDataCOFINSCodeCompanyDescri','BRTaxesDataCOFINSType','BRTaxesDataIPITaxable','BRTaxesDataIPIPerc','BRTaxesDataIPIValue','BRTaxesDataIPIFiscalValue','BRTaxesDataIPICode','BRTaxesDataIPICodeDescri','BRTaxesDataIPICodeCompany','BRTaxesDataIPICodeCompanyDescri','BRTaxesDataIPIType','BRTaxesDataSUFRAMATaxable','BRTaxesDataSUFRAMAPerc','BRTaxesDataSUFRAMAValue','BRTaxesDataSIMPLESTaxable','BRTaxesDataSIMPLESPerc','BRTaxesDataSIMPLESValue','BRTaxesDataSIMPLESCode','BRTaxesDataSIMPLESCodeDescri','BRTaxesDataSIMPLESCodeCompany','BRTaxesDataSIMPLESCodeCompanyDescri','BRTaxesDataICMSTaxable','BRTaxesDataICMSPerc','BRTaxesDataICMSValue','BRTaxesDataICMSRedPerc','BRTaxesDataICMSFiscalValue','BRTaxesDataICMSCode','BRTaxesDataICMSCodeDescri','BRTaxesDataICMSCodeCompany','BRTaxesDataICMSCodeCompanyDescri','BRTaxesDataICMSType','BRTaxesDataICMSDestTaxable','BRTaxesDataICMSDestPerc','BRTaxesDataICMSDestValue','BRTaxesDataICMSDestTempPerc','BRTaxesDataICMSInterPerc','BRTaxesDataICMSOrigValue','BRTaxesDataICMSFCPValue','BRTaxesDataICMSFCPPerc','BRTaxesDataICMSSTTaxable','BRTaxesDataICMSSTPerc','BRTaxesDataICMSSTValue','BRTaxesDataICMSSTRedPerc','BRTaxesDataICMSSTFiscalValue','BRTaxesDataMVAPerc','BRTaxesDataICMSSTToBeCompPerc','BRTaxesDataICMSSTCode','BRTaxesDataICMSSTCodeDescri','BRTaxesDataICMSSTType','BRTaxesDataICMSExTaxable','BRTaxesDataICMSExPerc','BRTaxesDataICMSExValue','BRTaxesDataICMSExCode','BRTaxesDataICMSExCodeDescri','BRTaxesDataICMSDefTaxable','BRTaxesDataICMSDefPerc','BRTaxesDataICMSDefValue','BRTaxesDataICMSDefCode','BRTaxesDataICMSDefCodeDescri','BRTaxesDataLog','BRTaxesDataMsg']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_BR_TAX_DATAFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_BR_TAX_DATAComponent, resolver);
    }
} 