import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_TO_GENERATIONService } from './IDD_TO_GENERATION.service';

@Component({
    selector: 'tb-IDD_TO_GENERATION',
    templateUrl: './IDD_TO_GENERATION.component.html',
    providers: [IDD_TO_GENERATIONService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_TO_GENERATIONComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_TO_GENERATION_SEL_DOCTYPE_itemSource: any;

    constructor(document: IDD_TO_GENERATIONService,
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
        this.IDC_TO_GENERATION_SEL_DOCTYPE_itemSource = {}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['bTRAll','bTRSel','sTRFrom','sTRTo','bDateAll','bDateSel','dDateFrom','dDateTo','bDocTypeAll','bDocTypeSel','eDocType','bMOAll','bMOSel','sMOFrom','sMOTo','bItemAll','bItemSel','sItemFrom','sItemTo','DBTTOGeneration'],'DBTTOGeneration':['Selected','StatusBmp','RequiredDate','TRNumber','TRStatus','MONo','DocumentType','DocumentNumber','Item','Storage','Lot','UoM','RequiredQty','ReleasedQty','ProcessedQty','ConfirmedTOQty'],'HKLItemsBE':['Description'],'HKLStorages':['Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_TO_GENERATIONFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_TO_GENERATIONComponent, resolver);
    }
} 