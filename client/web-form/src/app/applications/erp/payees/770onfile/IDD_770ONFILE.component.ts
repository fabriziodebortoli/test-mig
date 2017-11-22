import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_770ONFILEService } from './IDD_770ONFILE.service';

@Component({
    selector: 'tb-IDD_770ONFILE',
    templateUrl: './IDD_770ONFILE.component.html',
    providers: [IDD_770ONFILEService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_770ONFILEComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_CU_HEIR_CODE_itemSource: any;

    constructor(document: IDD_770ONFILEService,
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
        this.IDC_CU_HEIR_CODE_itemSource = {
  "name": "ItemSourceXml",
  "namespace": "ItemSource.Framework.TbGes.TbGes.ItemSourceXml",
  "parameter": "DataFile.ERP.Payees.PositionCode"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['FromPaymentDateFee','ToPaymentDateFee','FromSupplier','CustSuppDescription','CU_Intermediary','CU_CommitDate','MadeTaxpayer','ExtraordinaryEvents','INPS_FiscalCode','Progressive','DeclarationType','SubstituteFiscalCode','Software','Heir_FiscalCode','Heir_Code_XML','CU_SubstituteFiscalCode','CU_Ordinary','CU_ProtocolTel','CU_Sostitutive','CU_Cancellation','CU_ProtocolDoc','FileNameComplete','bOneFileForCertification','bOneReportForCertification','b770AllRecord','DBTSummaryDetail'],'DBTSummaryDetail':['l_LineSummaryDescription']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_770ONFILEFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_770ONFILEComponent, resolver);
    }
} 