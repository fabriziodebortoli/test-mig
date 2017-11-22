import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_ECODELETIONService } from './IDD_ECODELETION.service';

@Component({
    selector: 'tb-IDD_ECODELETION',
    templateUrl: './IDD_ECODELETION.component.html',
    providers: [IDD_ECODELETIONService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_ECODELETIONComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_ECODELETIONService,
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
		boService.appendToModelStructure({'global':['bNotItemSelection','bItemSelection','ItemFrom','ItemTo','bNotVariantSelection','bVariantSelection','VariantFrom','VariantTo','bNotECOSelection','bECOSelection','ECOFrom','bNotDateSelection','bDateSelection','DateFrom','bAllECO','bOnlyAuto','ECODeletions'],'ECODeletions':['ECODelSelected','ECODelBmp','ECONo','ECORevision','ECOStatus','ECOAutomaticallyGenerated','BOM','BOM','Variant','Description','ECOCreationDate','ECOConfirmationDate','ECOExecutionDate','ECOExecutionSignature','ECOCheckDate','ECOCheckSignature','ECOApprovalDate','ECOApprovalSignature','ECONotes']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_ECODELETIONFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_ECODELETIONComponent, resolver);
    }
} 