import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_ECOService } from './IDD_ECO.service';

@Component({
    selector: 'tb-IDD_ECO',
    templateUrl: './IDD_ECO.component.html',
    providers: [IDD_ECOService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_ECOComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_ECO_ROW_TYPE_PROD_COMPONENT_itemSource: any;
public IDC_ECO_ROW_UOM_itemSource: any;
public IDC_ECO_ROW_STEP_itemSource: any;
public IDC_LABOUR_DETAILS_ESTIMATED_LABOUR_TYPE_itemSource: any;

    constructor(document: IDD_ECOService,
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
        this.IDC_ECO_ROW_TYPE_PROD_COMPONENT_itemSource = {
  "name": "ComponentTypeEnumCombo",
  "namespace": "ERP.BillOfMaterials.Components.ComponentTypeItemSource"
}; 
this.IDC_ECO_ROW_UOM_itemSource = {
  "name": "UnitsOfMeasureComponentComboBoxEco",
  "namespace": "ERP.BillOfMaterialsPlus.Documents.UnitsOfMeasureComponentComboBoxEco"
}; 
this.IDC_ECO_ROW_STEP_itemSource = {
  "name": "DNRtgStepIntCombo",
  "namespace": "ERP.BillOfMaterials.Documents.RtgStepSentItemSource"
}; 
this.IDC_LABOUR_DETAILS_ESTIMATED_LABOUR_TYPE_itemSource = {
  "name": "eLabourTypeEnumCombo",
  "namespace": "ERP.Routing.Components.LabourLabourTypeItemSource"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'BillOfMaterials':['ECONo','ECORevision','ECOAutomaticallyGenerated','BOM','Description','ECOCreationDate','Variant','ECOConfirmationDate','ECORevision','ECOAutomaticallyGenerated','Description','ECOCreationDate','UoM','Disabled','SF','UsePercQty','SalesDocOnly','Configurable','InProduction','CreationDate','LastModificationDate','DwgDrawing','DwgPosition','DwgNotes','ECOExecutionDate','ECOExecutionSignature','ECOApprovalDate','ECOApprovalSignature','ECOCheckDate','ECOCheckSignature','ECONotes','ECOImagePath'],'global':['ECOStatusDescription','ImageStatusECO','DesignBOMOld','DesignBOM','BOMComponents','BOMRoutings','__DBTLabour','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'ECOHistory':['UoM','Disabled','SF','UsePercQty','SalesDocOnly','Configurable','InProduction','DwgDrawing','DwgPosition','DwgNotes'],'HKLECODrawings':['Description','Description'],'BOMComponents':['Bmp','VariationType','ECOLine','ComponentType','Semifinished','ValidityStartingDate','ValidityEndingDate','UseInOperation','IsKanban'],'BOMRoutings':['Bmp','BmpType','VariationType'],'@DBTLabour':['__ePhase','__bIsWorker','__eResourceType','__sResourceCode','__nWorkerID','__sResourceDescription','__eLabourType','__bIsPercent','__nAttendancePerc','__nWorkingTime','__dDate','__nNoOfResources'],'HKLECOTeamsExec':['Description'],'HKLECOTeamsAppr':['Description'],'HKLECOTeamsCheck':['Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_ECOFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_ECOComponent, resolver);
    }
} 