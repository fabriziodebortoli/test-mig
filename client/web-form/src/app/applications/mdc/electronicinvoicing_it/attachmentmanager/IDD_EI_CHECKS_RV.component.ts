import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_EI_CHECKS_RVService } from './IDD_EI_CHECKS_RV.service';

@Component({
    selector: 'tb-IDD_EI_CHECKS_RV',
    templateUrl: './IDD_EI_CHECKS_RV.component.html',
    providers: [IDD_EI_CHECKS_RVService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_EI_CHECKS_RVComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_EI_CHECKS_RVService,
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
        
        		this.bo.appendToModelStructure({'global':['Attachment_DocType','Attachment_DocNo','Attachment_DocDate','Attachment_EIStatus','Attachment_TaxJournal','Attachment_strIPACode','Attachment_CustSupp','Attachment_strCompanyName','Attachment_strTaxIdNumber','Attachment_strFiscalCode','Attachment_strAdminRef','Attachment'],'Attachment':['TEIAttachDetail_P02','TEIAttachDetail_P03']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_EI_CHECKS_RVFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_EI_CHECKS_RVComponent, resolver);
    }
} 