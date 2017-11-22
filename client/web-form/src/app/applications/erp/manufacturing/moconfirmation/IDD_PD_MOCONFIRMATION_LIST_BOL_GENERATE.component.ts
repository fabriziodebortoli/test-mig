import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_PD_MOCONFIRMATION_LIST_BOL_GENERATEService } from './IDD_PD_MOCONFIRMATION_LIST_BOL_GENERATE.service';

@Component({
    selector: 'tb-IDD_PD_MOCONFIRMATION_LIST_BOL_GENERATE',
    templateUrl: './IDD_PD_MOCONFIRMATION_LIST_BOL_GENERATE.component.html',
    providers: [IDD_PD_MOCONFIRMATION_LIST_BOL_GENERATEService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_PD_MOCONFIRMATION_LIST_BOL_GENERATEComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_MOCONFIRMATION_PREPRINTDOCNO_REG_CODETYPE_ITEM_itemSource: any;

    constructor(document: IDD_PD_MOCONFIRMATION_LIST_BOL_GENERATEService,
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
        this.IDC_MOCONFIRMATION_PREPRINTDOCNO_REG_CODETYPE_ITEM_itemSource = {
  "name": "ItemTypeEnumCombo",
  "namespace": "ERP.Manufacturing.Documents.ItmTypeItemSource"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['NrDoc','DocDate','DocQty','ItemType']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_PD_MOCONFIRMATION_LIST_BOL_GENERATEFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_PD_MOCONFIRMATION_LIST_BOL_GENERATEComponent, resolver);
    }
} 