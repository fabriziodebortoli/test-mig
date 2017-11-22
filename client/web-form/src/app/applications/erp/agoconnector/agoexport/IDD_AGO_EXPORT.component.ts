import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_AGO_EXPORTService } from './IDD_AGO_EXPORT.service';

@Component({
    selector: 'tb-IDD_AGO_EXPORT',
    templateUrl: './IDD_AGO_EXPORT.component.html',
    providers: [IDD_AGO_EXPORTService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_AGO_EXPORTComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_AGO_EXPORTService,
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
		boService.appendToModelStructure({'global':['CompanyCode','Application','DateDocLastExport','DateDocFrom','DateDocTo','bAlreadyExported','bExportPureJE','bExportDocIssued','bExportDocRecvd','AGOExportLog','AGOSubAccountLink','AGOAccReasonsLink','AGOAccTemplatesLink','AGOTaxCodesLink','CompanyCode','Application','DateDocLastExport','DocLastExportNo','FilePathExport','strOutput'],'AGOExportLog':['l_LogExport_TransactionType','l_LogExport_PostingDate','l_LogExport_DocNo','l_LogExport_AccTpl','l_LogExport_Notes'],'AGOSubAccountLink':['Account','Description','AGOSubAccount'],'HKLAGOSubAccounts':['Description'],'AGOAccReasonsLink':['Reason','Description','AGOAccReason'],'HKLAGOAccReasons':['Description'],'AGOAccTemplatesLink':['Template','Description','AGOAccReason'],'HKLAGOAccReasonsTpl':['Description'],'AGOTaxCodesLink':['TaxCode','Description','AGOLawCode','AGOTaxCode'],'HKLAGOLawCodes':['Description'],'HKLAGOTaxCodes':['Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_AGO_EXPORTFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_AGO_EXPORTComponent, resolver);
    }
} 