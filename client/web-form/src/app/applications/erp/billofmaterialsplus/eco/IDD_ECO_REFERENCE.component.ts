import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_ECO_REFERENCEService } from './IDD_ECO_REFERENCE.service';

@Component({
    selector: 'tb-IDD_ECO_REFERENCE',
    templateUrl: './IDD_ECO_REFERENCE.component.html',
    providers: [IDD_ECO_REFERENCEService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_ECO_REFERENCEComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_ECO_REFERENCE_USE_IN_STEP_itemSource: any;

    constructor(document: IDD_ECO_REFERENCEService,
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
        this.IDC_ECO_REFERENCE_USE_IN_STEP_itemSource = {
  "name": "UseInStepIntCombo",
  "namespace": "ERP.BillOfMaterialsPlus.Documents.RtgStepSentItemSource"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['UseInStep','bInsertBefore','bInsertAfter']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_ECO_REFERENCEFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_ECO_REFERENCEComponent, resolver);
    }
} 