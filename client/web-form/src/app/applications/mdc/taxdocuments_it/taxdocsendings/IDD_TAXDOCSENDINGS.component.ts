import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_TAXDOCSENDINGSService } from './IDD_TAXDOCSENDINGS.service';

@Component({
    selector: 'tb-IDD_TAXDOCSENDINGS',
    templateUrl: './IDD_TAXDOCSENDINGS.component.html',
    providers: [IDD_TAXDOCSENDINGSService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_TAXDOCSENDINGSComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_TAXDOCUMENTSSENDING_SENDINGSTATUS_itemSource: any;

    constructor(document: IDD_TAXDOCSENDINGSService,
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
        this.IDC_TAXDOCUMENTSSENDING_SENDINGSTATUS_itemSource = {
  "name": "SendingStatusCADIItemSource",
  "namespace": "MDC.TaxDocuments_IT.Documents.SendingStatusCADIItemSource"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'TaxDocSendings':['TaxDocSendingNo','SendingType','FromDate','ToDate','SetupDate','SendingDate','SendingStatus','IDFile','ManuallyUpdate'],'global':['TaxDocSendingsDetails','DBTEventViewer','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'TaxDocSendingsDetails':['Line','l_TEnhTaxDocSendingsDetails_P1','l_TEnhTaxDocSendingsDetails_P2','l_TEnhTaxDocSendingsDetails_P9','l_TEnhTaxDocSendingsDetails_P3','l_TEnhTaxDocSendingsDetails_P4','l_TEnhTaxDocSendingsDetails_P5','l_TEnhTaxDocSendingsDetails_P6','l_TEnhTaxDocSendingsDetails_P7','l_TEnhTaxDocSendingsDetails_P8','l_TEnhTaxDocSendingsDetails_P10'],'DBTEventViewer':['EventDate','Event_Type','Event_Description','Event_XML','Event_String1','Event_String2']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_TAXDOCSENDINGSFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_TAXDOCSENDINGSComponent, resolver);
    }
} 