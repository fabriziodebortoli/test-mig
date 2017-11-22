import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_SOSDOCSENDER_WIZARDService } from './IDD_SOSDOCSENDER_WIZARD.service';

@Component({
    selector: 'tb-IDD_SOSDOCSENDER_WIZARD',
    templateUrl: './IDD_SOSDOCSENDER_WIZARD.component.html',
    providers: [IDD_SOSDOCSENDER_WIZARDService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_SOSDOCSENDER_WIZARDComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_SOSDOCSENDER_DOCCLASS_COMBO_itemSource: any;
public IDC_SOSDOCSENDER_DOCTYPE_COMBO_itemSource: any;
public IDC_SOSDOCSENDER_TAXJOURNAL_COMBO_itemSource: any;
public IDC_SOSDOCSENDER_FISCALYEAR_COMBO_itemSource: any;

    constructor(document: IDD_SOSDOCSENDER_WIZARDService,
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
        this.IDC_SOSDOCSENDER_DOCCLASS_COMBO_itemSource = {
  "name": "SOSDocClassesItemSource",
  "namespace": "Extensions.EasyAttachment.TbDMS.SOSDocClassesItemSource"
}; 
this.IDC_SOSDOCSENDER_DOCTYPE_COMBO_itemSource = {
  "name": "SOSDocTypeItemSource",
  "namespace": "Extensions.EasyAttachment.TbDMS.SOSDocTypeItemSource"
}; 
this.IDC_SOSDOCSENDER_TAXJOURNAL_COMBO_itemSource = {
  "name": "SOSTaxJournalItemSource",
  "namespace": "Extensions.EasyAttachment.TbDMS.SOSTaxJournalItemSource"
}; 
this.IDC_SOSDOCSENDER_FISCALYEAR_COMBO_itemSource = {
  "name": "SOSFiscalYearItemSource",
  "namespace": "Extensions.EasyAttachment.TbDMS.SOSFiscalYearItemSource"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['bSendDocToSOS','bExcludeDocFromSOS','DocumentClass','DocumentType','TaxJournal','FiscalYear','bOnlyMainDoc','bDocIdle','bDocToResend','SOSStartingDate','SOSEndingDate','SOSAllCustSupp','SOSCustSuppSel','SOSFromCustSupp','SOSToCustSupp','SOSAllDocNo','SOSDocNoSel','SOSFromDocNo','SOSToDocNo','DBTSOSDocuments','DBTSOSElaboration','ElaborationMessage','nCurrentElement'],'DBTSOSDocuments':['VIsSelected','VFileName','VDocumentType','VDescriptionKeys','VDocumentStatus','VAttachmentID'],'DBTSOSElaboration':['VMsgBmp','VMessage']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_SOSDOCSENDER_WIZARDFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_SOSDOCSENDER_WIZARDComponent, resolver);
    }
} 