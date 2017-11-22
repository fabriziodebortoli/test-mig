﻿import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_BOM_ECO_VARService } from './IDD_BOM_ECO_VAR.service';

@Component({
    selector: 'tb-IDD_BOM_ECO_VAR',
    templateUrl: './IDD_BOM_ECO_VAR.component.html',
    providers: [IDD_BOM_ECO_VARService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_BOM_ECO_VARComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_LABOUR_DETAILS_ESTIMATED_LABOUR_TYPE_itemSource: any;

    constructor(document: IDD_BOM_ECO_VARService,
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
		boService.appendToModelStructure({'BomWithECO':['BOM'],'BOMWithECO':['Description'],'global':['sHeaderVariant','DBTVariantComponentsWithECO','DBTVariantRoutingWithECO','__DBTLabour','BOMComponentsWithECO','BOMRoutingWithECO','__DBTLabour','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'DBTVariantComponentsWithECO':['SubstType','ComponentType','Component','Description','Qty','UoM','FixedComponent','ScrapQty','ScrapUM','Variant','ValidityStartingDate','ValidityEndingDate','DNRtgStep','Notes'],'DBTVariantRoutingWithECO':['SubstType','RtgStep','Alternate','AltRtgStep','Operation','Notes','IsWC','WC','SetupTime','LineTypeInDN','ProcessingTime','TotalTime','Qty'],'@DBTLabour':['__ePhase','__bIsWorker','__eResourceType','__sResourceCode','__nWorkerID','__sResourceDescription','__eLabourType','__bIsPercent','__nAttendancePerc','__nWorkingTime','__dDate','__nNoOfResources','__ePhase','__bIsWorker','__eResourceType','__sResourceCode','__nWorkerID','__sResourceDescription','__eLabourType','__bIsPercent','__nAttendancePerc','__nWorkingTime','__dDate','__nNoOfResources'],'BOMComponentsWithECO':['ComponentType','Component','Description','Qty','UoM','FixedComponent','Valorize','Waste','ScrapQty','ScrapUM','Variant','ValidityStartingDate','ValidityEndingDate','DNRtgStep','Notes'],'BOMRoutingWithECO':['RtgStep','Alternate','AltRtgStep','Operation','Notes','IsWC','WC','SetupTime','LineTypeInDN','ProcessingTime','TotalTime','Qty']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_BOM_ECO_VARFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_BOM_ECO_VARComponent, resolver);
    }
} 