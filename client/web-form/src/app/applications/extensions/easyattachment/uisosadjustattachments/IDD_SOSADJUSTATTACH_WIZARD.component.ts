import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_SOSADJUSTATTACH_WIZARDService } from './IDD_SOSADJUSTATTACH_WIZARD.service';

@Component({
    selector: 'tb-IDD_SOSADJUSTATTACH_WIZARD',
    templateUrl: './IDD_SOSADJUSTATTACH_WIZARD.component.html',
    providers: [IDD_SOSADJUSTATTACH_WIZARDService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_SOSADJUSTATTACH_WIZARDComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_SOSADJUSTATTACH_DOCCLASS_COMBO_itemSource: any;
public IDC_SOSADJUSTATTACH_DOCTYPE_COMBO_itemSource: any;
public IDC_SOSADJUSTATTACH_FISCALYEAR_COMBO_itemSource: any;

    constructor(document: IDD_SOSADJUSTATTACH_WIZARDService,
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
        this.IDC_SOSADJUSTATTACH_DOCCLASS_COMBO_itemSource = {
  "name": "SOSDocClassesItemSource",
  "namespace": "Extensions.EasyAttachment.TbDMS.SOSDocClassesItemSource"
}; 
this.IDC_SOSADJUSTATTACH_DOCTYPE_COMBO_itemSource = {
  "name": "SOSDocTypeItemSource",
  "namespace": "Extensions.EasyAttachment.TbDMS.SOSDocTypeItemSource"
}; 
this.IDC_SOSADJUSTATTACH_FISCALYEAR_COMBO_itemSource = {
  "name": "SOSFiscalYearItemSource",
  "namespace": "Extensions.EasyAttachment.TbDMS.SOSFiscalYearItemSource"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['DocumentClass','DocumentType','FiscalYear','SOSStartingDate','SOSEndingDate','SOSAllCustSupp','SOSCustSuppSel','SOSFromCustSupp','SOSToCustSupp','SOSAllDocNo','SOSDocNoSel','SOSFromDocNo','SOSToDocNo','DBTSOSDocuments','DBTSOSElaboration','ElaborationMessage','nCurrentElement'],'DBTSOSDocuments':['VIsSelected','VFileName','VDocumentType','VDescriptionKeys','VAttachmentID'],'DBTSOSElaboration':['VMsgBmp','VMessage']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_SOSADJUSTATTACH_WIZARDFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_SOSADJUSTATTACH_WIZARDComponent, resolver);
    }
} 