import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_EI_HU_TRASMISSIONService } from './IDD_EI_HU_TRASMISSION.service';

@Component({
    selector: 'tb-IDD_EI_HU_TRASMISSION',
    templateUrl: './IDD_EI_HU_TRASMISSION.component.html',
    providers: [IDD_EI_HU_TRASMISSIONService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_EI_HU_TRASMISSIONComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_EI_HU_TRASMISSIONService,
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
        
        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['DateFrom','DateTo','bAllDocNo','bDocNoSel','FromDocNo','ToDocNo','DirToSaveFileXML','EIChecks'],'EIChecks':['DocumentType','DocNo','DocumentDate','CustSupp','TEIMDCDocDetail_P04','TaxJournal','TEIMDCDocDetail_P03','TEIMDCDocDetail_P05','TEIMDCDocDetail_P23']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_EI_HU_TRASMISSIONFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_EI_HU_TRASMISSIONComponent, resolver);
    }
} 