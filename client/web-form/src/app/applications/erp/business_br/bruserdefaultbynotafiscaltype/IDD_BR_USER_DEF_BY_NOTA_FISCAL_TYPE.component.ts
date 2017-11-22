import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_BR_USER_DEF_BY_NOTA_FISCAL_TYPEService } from './IDD_BR_USER_DEF_BY_NOTA_FISCAL_TYPE.service';

@Component({
    selector: 'tb-IDD_BR_USER_DEF_BY_NOTA_FISCAL_TYPE',
    templateUrl: './IDD_BR_USER_DEF_BY_NOTA_FISCAL_TYPE.component.html',
    providers: [IDD_BR_USER_DEF_BY_NOTA_FISCAL_TYPEService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_BR_USER_DEF_BY_NOTA_FISCAL_TYPEComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_BR_USER_DEF_BY_NOTA_FISCAL_TYPEService,
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
        
        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['DisableRollBack_Cust_Issue','UpdateAccounting_Cust_Issue','UpdateInventory_Cust_Issue','Issue_Cust_Issue','DisableUpdIssue_Cust_Issue','PostPymntSched_Cust_Issue','DisableUpdPostPymntSched_Cust_Issue','PostInventory_Cust_Issue','DisableUpdPostInventory_Cust_Issue','Printer_Cust_Issue','DisableUpdPrinter_Cust_Issue','PrintPreview_Cust_Issue','SendEMail_Cust_Issue','DisableUpdSendEMail_Cust_Issue','Archive_Cust_Issue','DisableUpdArchive_Cust_Issue','DisableRollBack_Supp_Issue','UpdateAccounting_Supp_Issue','UpdateInventory_Supp_Issue','Issue_Supp_Issue','DisableUpdIssue_Supp_Issue','PostPymntSched_Supp_Issue','DisableUpdPostPymntSched_Supp_Issue','PostInventory_Supp_Issue','DisableUpdPostInventory_Supp_Issue','Printer_Supp_Issue','DisableUpdPrinter_Supp_Issue','PrintPreview_Supp_Issue','SendEMail_Supp_Issue','DisableUpdSendEMail_Supp_Issue','Archive_Supp_Issue','DisableUpdArchive_Supp_Issue','DisableRollBack_Cust_Receipt','UpdateAccounting_Cust_Receipt','UpdateInventory_Cust_Receipt','Issue_Cust_Receipt','DisableUpdIssue_Cust_Receipt','PostPymntSched_Cust_Receipt','DisableUpdPostPymntSched_Cust_Receipt','PostInventory_Cust_Receipt','DisableUpdPostInventory_Cust_Receipt','Printer_Cust_Receipt','DisableUpdPrinter_Cust_Receipt','PrintPreview_Cust_Receipt','SendEMail_Cust_Receipt','DisableUpdSendEMail_Cust_Receipt','Archive_Cust_Receipt','DisableUpdArchive_Cust_Receipt','DisableRollBack_Supp_Receipt','UpdateAccounting_Supp_Receipt','UpdateInventory_Supp_Receipt','Issue_Supp_Receipt','DisableUpdIssue_Supp_Receipt','PostPymntSched_Supp_Receipt','DisableUpdPostPymntSched_Supp_Receipt','PostInventory_Supp_Receipt','DisableUpdPostInventory_Supp_Receipt','Printer_Supp_Receipt','DisableUpdPrinter_Supp_Receipt','PrintPreview_Supp_Receipt','SendEMail_Supp_Receipt','DisableUpdSendEMail_Supp_Receipt','Archive_Supp_Receipt','DisableUpdArchive_Supp_Receipt']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_BR_USER_DEF_BY_NOTA_FISCAL_TYPEFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_BR_USER_DEF_BY_NOTA_FISCAL_TYPEComponent, resolver);
    }
} 