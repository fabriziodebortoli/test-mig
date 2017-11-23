import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_ECOCONFIRMATIONService } from './IDD_ECOCONFIRMATION.service';

@Component({
    selector: 'tb-IDD_ECOCONFIRMATION',
    templateUrl: './IDD_ECOCONFIRMATION.component.html',
    providers: [IDD_ECOCONFIRMATIONService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_ECOCONFIRMATIONComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_ECOCONFIRMATIONService,
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
		boService.appendToModelStructure({'global':['bNotItemSelection','bItemSelection','ItemFrom','ItemTo','bNotVariantSelection','bVariantSelection','VariantFrom','VariantTo','bNotECOSelection','bECOSelection','ECOFrom','ECOTo','bNotDateSelection','bDateSelection','DateFrom','DateTo','bAllECO','bOnlyAuto','ECOConfirmations'],'ECOConfirmations':['ECOConfSelected','ECOConfBmp','ECONo','ECORevision','ECOStatus','ECOAutomaticallyGenerated','BOM','BOM','Variant','Description','ECOCreationDate','ECOExecutionDate','ECOExecutionSignature','ECOExecutionSignature','ECOCheckDate','ECOCheckSignature','ECOApprovalDate','ECOApprovalSignature','ECONotes']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_ECOCONFIRMATIONFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_ECOCONFIRMATIONComponent, resolver);
    }
} 