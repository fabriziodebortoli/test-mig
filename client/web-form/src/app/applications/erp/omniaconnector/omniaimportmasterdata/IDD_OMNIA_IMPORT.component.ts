import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_OMNIA_IMPORTService } from './IDD_OMNIA_IMPORT.service';

@Component({
    selector: 'tb-IDD_OMNIA_IMPORT',
    templateUrl: './IDD_OMNIA_IMPORT.component.html',
    providers: [IDD_OMNIA_IMPORTService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_OMNIA_IMPORTComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_OMNIA_IMPORTService,
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
		boService.appendToModelStructure({'global':['bEnableMaster','DateLastImportMaster','bEnableImportAccount','OMNIASubAccountsCount','bUseOMNIACoA','bEnableImportReasons','OMNIAAccReasonsCount','FilePathImport','FileNameImport','bEnableCodes','DateLastImportCodes','bEnableImportTaxCode','OMNIATaxCodesCount','bEnableImportLawCode','OMNIALawCodesCount','bEnableImportIntraOpCode','OMNIAIntraCodesCount','FilePathImportCodes','FileNameImportCodes','strOutput']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_OMNIA_IMPORTFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_OMNIA_IMPORTComponent, resolver);
    }
} 