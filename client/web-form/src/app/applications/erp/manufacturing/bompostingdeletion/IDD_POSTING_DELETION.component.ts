import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_POSTING_DELETIONService } from './IDD_POSTING_DELETION.service';

@Component({
    selector: 'tb-IDD_POSTING_DELETION',
    templateUrl: './IDD_POSTING_DELETION.component.html',
    providers: [IDD_POSTING_DELETIONService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_POSTING_DELETIONComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_POSTING_DELETIONService,
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
		boService.appendToModelStructure({'global':['AllBOMNo','BOMNoSel','FromBOMPostingNo','ToBOMPostingNo','AllBOMDate','BOMDateSel','FromDocDate','ToDocDate','AllItem','SelItem','FromItem','ToItem','AllVariant','SelVariant','FromVariant','ToVariant','AllJob','SelJob','FromJob','ToJob','DBTBOMPostingDeletion'],'DBTBOMPostingDeletion':['TEnhPostingDeletionSelection','BOMPostingNo','DocumentNo','DocumentDate','PostingDate','BOM','Variant','Job','Notes']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_POSTING_DELETIONFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_POSTING_DELETIONComponent, resolver);
    }
} 