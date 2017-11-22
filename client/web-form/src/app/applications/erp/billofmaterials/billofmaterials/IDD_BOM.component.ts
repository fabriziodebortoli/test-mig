import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_BOMService } from './IDD_BOM.service';

@Component({
    selector: 'tb-IDD_BOM',
    templateUrl: './IDD_BOM.component.html',
    providers: [IDD_BOMService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_BOMComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_BOM_CODETYPE_BOM_itemSource: any;
public IDC_BOM_DISPLAY_PROD_itemSource: any;
public IDC_BOM_UNIMIS_itemSource: any;
public IDC_BOM_ANSWRESP_BE_CODETYPE_COMPO_itemSource: any;
public IDC_BOM_BE_ANSWRESP_UNI_MIS_itemSource: any;
public IDC_BOM_BE_ANSWRESP_UNI_MIS_WASTE_itemSource: any;
public IDC_LABOUR_DETAILS_ESTIMATED_LABOUR_TYPE_itemSource: any;
public IDC_TOOLS_MAN_RTGSTEP_itemSource: any;
public IDC_TOOLS_MAN_ALT_itemSource: any;
public IDC_TOOLS_MAN_RTGSTEP_ALT_RTGSTEP_itemSource: any;

    constructor(document: IDD_BOMService,
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
        this.IDC_BOM_CODETYPE_BOM_itemSource = {
  "allowChanges": false,
  "name": "CodeTypeEnumCombo",
  "namespace": "ERP.BillOfMaterials.Components.TypeBillOfMaterialsItemSource",
  "useProductLanguage": false
}; 
this.IDC_BOM_DISPLAY_PROD_itemSource = {
  "allowChanges": false,
  "name": "BOMStatusStrCombo",
  "namespace": "ERP.BillOfMaterials.Components.BOMStatusItemSource",
  "useProductLanguage": false
}; 
this.IDC_BOM_UNIMIS_itemSource = {
  "allowChanges": false,
  "name": "UnitsOfMeasureBOMComboBox",
  "namespace": "ERP.BillOfMaterials.Documents.UnitsOfMeasureBOMComboBox",
  "useProductLanguage": false
}; 
this.IDC_BOM_ANSWRESP_BE_CODETYPE_COMPO_itemSource = {
  "name": "ComponentTypeEnumCombo",
  "namespace": "ERP.BillOfMaterials.Components.ComponentTypeItemSource"
}; 
this.IDC_BOM_BE_ANSWRESP_UNI_MIS_itemSource = {
  "name": "UoMCombo",
  "namespace": "ERP.BillOfMaterials.Documents.UnitsOfMeasureAnswRespComboBox"
}; 
this.IDC_BOM_BE_ANSWRESP_UNI_MIS_WASTE_itemSource = {
  "name": "WasteUoMCombo",
  "namespace": "ERP.BillOfMaterials.Documents.UnitsOfMeasureAnswRespComboBox"
}; 
this.IDC_LABOUR_DETAILS_ESTIMATED_LABOUR_TYPE_itemSource = {
  "name": "eLabourTypeEnumCombo",
  "namespace": "ERP.Routing.Components.LabourLabourTypeItemSource"
}; 
this.IDC_TOOLS_MAN_RTGSTEP_itemSource = {
  "name": "RtgStepIntCombo",
  "namespace": "ERP.ToolsManagement.AddOnsCore.RtgStepToolsComboBox"
}; 
this.IDC_TOOLS_MAN_ALT_itemSource = {
  "name": "AlternateStrCombo",
  "namespace": "ERP.ToolsManagement.AddOnsCore.AlternateToolsItemSource"
}; 
this.IDC_TOOLS_MAN_RTGSTEP_ALT_RTGSTEP_itemSource = {
  "name": "DNRtgStepIntCombo",
  "namespace": "ERP.ToolsManagement.AddOnsCore.AltRtgStepToolsItemSource"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'BillOfMaterials':['CodeType','BOM','Description','Disabled','UsePercQty','SF','UoM','Configurable','SalesDocOnly','LastModificationDate','CreationDate','Notes'],'global':['BOMStatus','TotPercentage','BOMComponents','BOMQuestionsAnswers','Internal','Outsourced','Supplier','BOMRoutings','__DBTLabour','DBTBOMECO','DBTToolsManagement','DBTBOMVariants','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg','DBTLinksTable','ToolBmp','FamilyBmp'],'BOMDrawings':['Drawing','Position','Drawing','Correction','Notes'],'HKLDrawings':['Description'],'BOMComponents':['IsBOM','Nature','Configurable','IsKanban'],'BOMQuestionsAnswers':['IsADefault','AnswerNo','AnswerDes','ComponentType','Component','Description','Variant','Qty','UoM','ToExplode','PercWaste','WastePerc','ScrapQty','ScrapUM','ValidityStartingDate','ValidityEndingDate','Notes'],'HKLSupplier':['CompanyName'],'BOMRoutings':['BOMStateBmp','Outsourced','SetupTime','ProcessingTime','TotalTime','Qty','QueueTime','LineTypeInDN','InHouseProcessingCost','Notes'],'@DBTLabour':['__ePhase','__bIsWorker','__eResourceType','__sResourceCode','__nWorkerID','__sResourceDescription','__eLabourType','__bIsPercent','__nAttendancePerc','__nWorkingTime','__dDate','__nNoOfResources'],'DBTBOMECO':['l_BmpStatus','ECONo','ECORevision','ECOStatus','ECOConfirmationDate','ECOCreationDate','ECONotes'],'DBTToolsManagement':['RtgStepTypeBmp','RtgStep','Alternate','AltRtgStep','Operation','OperationDescription','Usage','IsFamily','Tool','ProcessingType','ToolType','Fixed','UsageQuantity','UsageTime','ExclusiveUse','Source','SourceTool'],'HKLSelectorTools':['Description'],'DBTBOMVariants':['Variant','Item','FromConfigurator','Notes'],'DBTLinksTable':['Image','Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_BOMFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_BOMComponent, resolver);
    }
} 