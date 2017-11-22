import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_VARIANTService } from './IDD_VARIANT.service';

@Component({
    selector: 'tb-IDD_VARIANT',
    templateUrl: './IDD_VARIANT.component.html',
    providers: [IDD_VARIANTService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_VARIANTComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_LABOUR_DETAILS_ESTIMATED_LABOUR_TYPE_itemSource: any;

    constructor(document: IDD_VARIANTService,
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
        this.IDC_LABOUR_DETAILS_ESTIMATED_LABOUR_TYPE_itemSource = {
  "name": "eLabourTypeEnumCombo",
  "namespace": "ERP.Routing.Components.LabourLabourTypeItemSource"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'Variants':['Item','Variant','BOM','Notes','FromConfigurator'],'HKLItemsSelectNature':['Description'],'HKLBillOfMaterialsNoGhost':['Description'],'global':['VariantComponents','VariantRouting','__DBTLabour','DBTVariantsECO','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'@DBTLabour':['__ePhase','__bIsWorker','__eResourceType','__sResourceCode','__nWorkerID','__sResourceDescription','__eLabourType','__bIsPercent','__nAttendancePerc','__nWorkingTime','__dDate','__nNoOfResources'],'DBTVariantsECO':['l_BmpStatus','ECONo','ECORevision','ECOStatus','ECOConfirmationDate','ECOCreationDate','ECONotes']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_VARIANTFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_VARIANTComponent, resolver);
    }
} 