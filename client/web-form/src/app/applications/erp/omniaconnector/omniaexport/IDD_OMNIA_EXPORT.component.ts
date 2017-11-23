import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_OMNIA_EXPORTService } from './IDD_OMNIA_EXPORT.service';

@Component({
    selector: 'tb-IDD_OMNIA_EXPORT',
    templateUrl: './IDD_OMNIA_EXPORT.component.html',
    providers: [IDD_OMNIA_EXPORTService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_OMNIA_EXPORTComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_OMNIA_EXPORTService,
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
        
        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['CompanyCode','DateDocLastExport','DateDocFrom','DateDocTo','bAlreadyExported','bExportPureJE','bExportDocIssued','bExportDocRecvd','ExportLog','OMNIASubAccountLink','OMNIAAccReasonsLink','OMNIATaxCodesLink','CompanyCode','DateDocLastExport','DocLastExportNo','FilePathExport','FileNameExport','strOutput'],'ExportLog':['l_LogExport_TransactionType','l_LogExport_PostingDate','l_LogExport_DocNo','l_LogExport_AccTpl','l_LogExport_OMNIATag','l_LogExport_Notes'],'OMNIASubAccountLink':['Account','Description','OMNIASubAccount'],'HKLOMNIASubAccounts':['Description'],'OMNIAAccReasonsLink':['Reason','Description','OMNIAAccReason'],'HKLOMNIAAccReasons':['Description'],'OMNIATaxCodesLink':['TaxCode','Description','OMNIALawCode','OMNIATaxCode'],'HKLOMNIALawCodes':['Description'],'HKLOMNIATaxCodes':['Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_OMNIA_EXPORTFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_OMNIA_EXPORTComponent, resolver);
    }
} 