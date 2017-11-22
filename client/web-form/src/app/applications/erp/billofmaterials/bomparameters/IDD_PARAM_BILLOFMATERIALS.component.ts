import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_PARAM_BILLOFMATERIALSService } from './IDD_PARAM_BILLOFMATERIALS.service';

@Component({
    selector: 'tb-IDD_PARAM_BILLOFMATERIALS',
    templateUrl: './IDD_PARAM_BILLOFMATERIALS.component.html',
    providers: [IDD_PARAM_BILLOFMATERIALSService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_PARAM_BILLOFMATERIALSComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_BDY_JEGL_BMP_PRNOPERATIONIG_itemSource: any;

    constructor(document: IDD_PARAM_BILLOFMATERIALSService,
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
        this.IDC_BDY_JEGL_BMP_PRNOPERATIONIG_itemSource = {
  "name": "DocumentTypeCombo",
  "namespace": "ERP.BillOfMaterials.Services.SalesDocEnumComboBOM"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['BOMDocumentsParametersDBT','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'BOMDocumentsParametersDBT':['DocumentType','ExpandFirstLevelOnly','GenerateShortInvEntriesSet','RMClearingInvRsn','FPIssueToProdInvRsn','RMClearingProdInvRsn','RMReceiptInvRsn','WasteInvRsn','WasteDifferentItemInvRsn']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_PARAM_BILLOFMATERIALSFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_PARAM_BILLOFMATERIALSComponent, resolver);
    }
} 