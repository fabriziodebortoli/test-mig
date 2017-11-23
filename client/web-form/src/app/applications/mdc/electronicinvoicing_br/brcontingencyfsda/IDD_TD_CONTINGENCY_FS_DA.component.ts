import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_TD_CONTINGENCY_FS_DAService } from './IDD_TD_CONTINGENCY_FS_DA.service';

@Component({
    selector: 'tb-IDD_TD_CONTINGENCY_FS_DA',
    templateUrl: './IDD_TD_CONTINGENCY_FS_DA.component.html',
    providers: [IDD_TD_CONTINGENCY_FS_DAService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_TD_CONTINGENCY_FS_DAComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_TD_CONTINGENCY_FS_DAService,
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
		boService.appendToModelStructure({'global':['bNFForCust','bNFForSupp','Model','Series','bAllDate','bDateSel','StartingDate','EndingDate','bAllNum','bNumSel','StartingNum','EndingNum','Detail'],'Detail':['TEnhContin_bSelected','TEnhContin_DocNo','TEnhContin_DocDate','TEnhContin_CustSupp','TEnhContin_CustSuppName','TEnhContin_NotaFiscalStatus','TEnhContin_ElabResult']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_TD_CONTINGENCY_FS_DAFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_TD_CONTINGENCY_FS_DAComponent, resolver);
    }
} 