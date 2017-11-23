import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_BOM_ADD_REFService } from './IDD_BOM_ADD_REF.service';

@Component({
    selector: 'tb-IDD_BOM_ADD_REF',
    templateUrl: './IDD_BOM_ADD_REF.component.html',
    providers: [IDD_BOM_ADD_REFService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_BOM_ADD_REFComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_BOM_REFERENCE_OPERATION_itemSource: any;

    constructor(document: IDD_BOM_ADD_REFService,
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
        this.IDC_BOM_REFERENCE_OPERATION_itemSource = {
  "name": "OperationComboBox",
  "namespace": "ERP.BillOfMaterials.Documents.UseInOperationComboBox"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['Operation','bInsertBefore','bInsertAfter']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_BOM_ADD_REFFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_BOM_ADD_REFComponent, resolver);
    }
} 