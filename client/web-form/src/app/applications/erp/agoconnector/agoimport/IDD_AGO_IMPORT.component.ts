import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_AGO_IMPORTService } from './IDD_AGO_IMPORT.service';

@Component({
    selector: 'tb-IDD_AGO_IMPORT',
    templateUrl: './IDD_AGO_IMPORT.component.html',
    providers: [IDD_AGO_IMPORTService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_AGO_IMPORTComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_AGO_IMPORTService,
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
		boService.appendToModelStructure({'global':['DateLastImport','FilePathImport','bEnableImportAccount','AGOSubAccountsCount','bEnableImportReasons','AGOAccReasonsCount','bEnableImportTaxCode','AGOTaxCodesCount','bEnableImportLawCode','AGOLawCodesCount','bEnableImportIntraOpCode','AGOIntraCodesCount','strOutput']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_AGO_IMPORTFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_AGO_IMPORTComponent, resolver);
    }
} 