import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_MOCONFIRMATION_LIST_BOLService } from './IDD_MOCONFIRMATION_LIST_BOL.service';

@Component({
    selector: 'tb-IDD_MOCONFIRMATION_LIST_BOL',
    templateUrl: './IDD_MOCONFIRMATION_LIST_BOL.component.html',
    providers: [IDD_MOCONFIRMATION_LIST_BOLService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_MOCONFIRMATION_LIST_BOLComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_MOCONFIRMATION_LIST_BOL_CODETYPE_ITM_itemSource: any;

    constructor(document: IDD_MOCONFIRMATION_LIST_BOLService,
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
        this.IDC_MOCONFIRMATION_LIST_BOL_CODETYPE_ITM_itemSource = {
  "name": "ItemTypeEnumCombo",
  "namespace": "ERP.Manufacturing.Documents.ItmTypeItemSource"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['ProductionQty','PreprintedDocDetail','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'PreprintedDocDetail':['BillOfLadingNo','PreprintedDocNo','PostingDate','ProducedQty','ItemType','PostedToInventory']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_MOCONFIRMATION_LIST_BOLFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_MOCONFIRMATION_LIST_BOLComponent, resolver);
    }
} 