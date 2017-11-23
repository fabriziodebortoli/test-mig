import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_MO_FROM_CONFIRMService } from './IDD_MO_FROM_CONFIRM.service';

@Component({
    selector: 'tb-IDD_MO_FROM_CONFIRM',
    templateUrl: './IDD_MO_FROM_CONFIRM.component.html',
    providers: [IDD_MO_FROM_CONFIRMService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_MO_FROM_CONFIRMComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_MO_FROM_CONFIRM_CODETYPE_SPECIFICATOR_itemSource: any;

    constructor(document: IDD_MO_FROM_CONFIRMService,
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
        this.IDC_MO_FROM_CONFIRM_CODETYPE_SPECIFICATOR_itemSource = {
  "name": "SpecificatorTypeCombo",
  "namespace": "ERP.Inventory.Components.SpecTypeNoIgnoreCombo"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['SubcntBoLMOStepsList'],'SubcntBoLMOStepsList':['Selection','StateBmp','MONo','RtgStep','Alternate','AltRtgStep','ProductionQty','ProducedQty','MOToConfQtyToConfirm','UoM','MOStatus','MOToConfConfirm','BOM','Variant','MOToConfProducedQty','Storage','MOToConfSpecificatorType','MOToConfSpecificator','ToIssueProductionQuantity','SecondRate','SecondRateVariant','MOToConfSecondRateQuantity','SecondRateStorage','Scrap','ScrapVariant','MOToConfScrapDescri','MOToConfScrapQuantity','ScrapStorage','Job'],'HKLSecondRate':['Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_MO_FROM_CONFIRMFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_MO_FROM_CONFIRMComponent, resolver);
    }
} 