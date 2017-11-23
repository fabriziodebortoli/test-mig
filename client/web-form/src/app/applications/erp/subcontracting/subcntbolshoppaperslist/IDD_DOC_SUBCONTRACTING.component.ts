import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_DOC_SUBCONTRACTINGService } from './IDD_DOC_SUBCONTRACTING.service';

@Component({
    selector: 'tb-IDD_DOC_SUBCONTRACTING',
    templateUrl: './IDD_DOC_SUBCONTRACTING.component.html',
    providers: [IDD_DOC_SUBCONTRACTINGService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_DOC_SUBCONTRACTINGComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_DOC_SUBCONTRACTINGService,
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
        
        		this.bo.appendToModelStructure({'global':['bPurchaseOrder','bSubcntDN','bAll','bAlsoDescriptionLines','SubcntBoLShopPapersList'],'SubcntBoLShopPapersList':['Selection','BOMSubcontracting','DocumentNumber','Position','BOM','Variant','BOMDescri','Operation','OperationDescri','UoM','DocQty','ConfirmedQty','DocBoLQty','MONo','RtgStep','Alternate','AltRtgStep','Job','Customer','SaleOrdNo']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_DOC_SUBCONTRACTINGFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_DOC_SUBCONTRACTINGComponent, resolver);
    }
} 