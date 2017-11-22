import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_BOM_PROD_DATAService } from './IDD_BOM_PROD_DATA.service';

@Component({
    selector: 'tb-IDD_BOM_PROD_DATA',
    templateUrl: './IDD_BOM_PROD_DATA.component.html',
    providers: [IDD_BOM_PROD_DATAService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_BOM_PROD_DATAComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_BOM_PROD_BOM_BE_VALUATE_DETAIL_itemSource: any;
public IDC_BOM_PROD_COMP_BE_UOM_itemSource: any;
public IDC_BOM_PROD_COMP_BE_UOM_SUM_itemSource: any;
public IDC_BOM_PROD_COMP_BE_UOM_WASTE_itemSource: any;
public IDC_BOM_PROD_VAL_VALUATE_itemSource: any;
public IDC_BOM_PROD_RSN_WST_SAME_TYPE_F1_itemSource: any;
public IDC_BOM_PROD_RSN_WST_SAME_TYPE_F2_itemSource: any;
public IDC_BOM_PROD_RSN_WST_DIFF_TYPE_F1_itemSource: any;
public IDC_BOM_PROD_RSN_WST_DIFF_TYPE_F2_itemSource: any;
public IDC_BOM_PROD_RSN_RET_UNLD_TYPE_F1_itemSource: any;
public IDC_BOM_PROD_RSN_RET_UNLD_TYPE_F2_itemSource: any;
public IDC_BOM_PROD_RSN_RET_LOAD_TYPE_F1_itemSource: any;
public IDC_BOM_PROD_RSN_RET_LOAD_TYPE_F2_itemSource: any;
public IDC_BOM_PROD_RSN_RUN_UNLD_TYPE_F1_itemSource: any;
public IDC_BOM_PROD_RSN_RUN_UNLD_TYPE_F2_itemSource: any;
public IDC_BOM_PROD_RSN_RUN_LOAD_TYPE_F1_itemSource: any;
public IDC_BOM_PROD_RSN_RUN_LOAD_TYPE_F2_itemSource: any;

    constructor(document: IDD_BOM_PROD_DATAService,
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
        this.IDC_BOM_PROD_BOM_BE_VALUATE_DETAIL_itemSource = {
  "name": "ValueTypeCombo",
  "namespace": "ERP.Company.Components.ValuationInventoryCombo"
}; 
this.IDC_BOM_PROD_COMP_BE_UOM_itemSource = {
  "name": "UnitsOfMeasureSubstCompCombo",
  "namespace": "ERP.BillOfMaterials.Documents.UnitsOfMeasureSubstCompComboBox"
}; 
this.IDC_BOM_PROD_COMP_BE_UOM_SUM_itemSource = {
  "name": "UnitsOfMeasureSubstCompCombo",
  "namespace": "ERP.BillOfMaterials.Documents.UnitsOfMeasureSubstCompComboBox"
}; 
this.IDC_BOM_PROD_COMP_BE_UOM_WASTE_itemSource = {
  "name": "UnitsOfMeasureSubstCompCombo",
  "namespace": "ERP.BillOfMaterials.Documents.UnitsOfMeasureSubstCompComboBox"
}; 
this.IDC_BOM_PROD_VAL_VALUATE_itemSource = {
  "name": "ValueTypeCombo",
  "namespace": "ERP.Company.Components.ValuationInventoryCombo"
}; 
this.IDC_BOM_PROD_RSN_WST_SAME_TYPE_F1_itemSource = {
  "name": "SpecificatorTypeCombo",
  "namespace": "ERP.Inventory.Components.SpecTypeNoIgnoreCombo"
}; 
this.IDC_BOM_PROD_RSN_WST_SAME_TYPE_F2_itemSource = {
  "name": "SpecificatorTypeCombo",
  "namespace": "ERP.Inventory.Components.SpecTypeNoIgnoreCombo"
}; 
this.IDC_BOM_PROD_RSN_WST_DIFF_TYPE_F1_itemSource = {
  "name": "SpecificatorTypeCombo",
  "namespace": "ERP.Inventory.Components.SpecTypeNoIgnoreCombo"
}; 
this.IDC_BOM_PROD_RSN_WST_DIFF_TYPE_F2_itemSource = {
  "name": "SpecificatorTypeCombo",
  "namespace": "ERP.Inventory.Components.SpecTypeNoIgnoreCombo"
}; 
this.IDC_BOM_PROD_RSN_RET_UNLD_TYPE_F1_itemSource = {
  "name": "SpecificatorTypeCombo",
  "namespace": "ERP.Inventory.Components.SpecTypeNoIgnoreCombo"
}; 
this.IDC_BOM_PROD_RSN_RET_UNLD_TYPE_F2_itemSource = {
  "name": "SpecificatorTypeCombo",
  "namespace": "ERP.Inventory.Components.SpecTypeNoIgnoreCombo"
}; 
this.IDC_BOM_PROD_RSN_RET_LOAD_TYPE_F1_itemSource = {
  "name": "SpecificatorTypeCombo",
  "namespace": "ERP.Inventory.Components.SpecTypeNoIgnoreCombo"
}; 
this.IDC_BOM_PROD_RSN_RET_LOAD_TYPE_F2_itemSource = {
  "name": "SpecificatorTypeCombo",
  "namespace": "ERP.Inventory.Components.SpecTypeNoIgnoreCombo"
}; 
this.IDC_BOM_PROD_RSN_RUN_UNLD_TYPE_F1_itemSource = {
  "name": "SpecificatorTypeCombo",
  "namespace": "ERP.Inventory.Components.SpecTypeNoIgnoreCombo"
}; 
this.IDC_BOM_PROD_RSN_RUN_UNLD_TYPE_F2_itemSource = {
  "name": "SpecificatorTypeCombo",
  "namespace": "ERP.Inventory.Components.SpecTypeNoIgnoreCombo"
}; 
this.IDC_BOM_PROD_RSN_RUN_LOAD_TYPE_F1_itemSource = {
  "name": "SpecificatorTypeCombo",
  "namespace": "ERP.Inventory.Components.SpecTypeNoIgnoreCombo"
}; 
this.IDC_BOM_PROD_RSN_RUN_LOAD_TYPE_F2_itemSource = {
  "name": "SpecificatorTypeCombo",
  "namespace": "ERP.Inventory.Components.SpecTypeNoIgnoreCombo"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['bExecuteRun','bExecuteConf','bProducePlan','sPlanNo','bProduceBOM','bProduceSemiFinished','bSemiFinishedNetting','bSavePlan','bUseSemifinishedAsMaterials','RunDate','BOMProductionDetails','DBTBOMProductionCompDetails','DBTBOMLotComponentsDetail','bValPlan','bValBOM','bValComp','bValCustom','eValType','bEvaluateByLot','eValType','bEvaluateByLot','sNotes','nCurrentElement','GaugeDescription','ProgressViewer','WstSameReason','WstSameStrgF1','WstSameTypeF1','WstSameSpecF1','WstSameSpecF1','WstSameStrgF2','WstSameTypeF2','WstSameSpecF2','WstSameSpecF2','WstDiffReason','WstDiffStrgF1','WstDiffTypeF1','WstDiffSpecF1','WstDiffSpecF1','WstDiffStrgF2','WstDiffTypeF2','WstDiffSpecF2','WstDiffSpecF2','RetUnldReason','RetUnldStrgF1','RetLoadTypeF1','RetUnldSpecF1','RetUnldSpecF1','RetUnldStrgF2','RetUnldTypeF2','RetUnldSpecF2','RetUnldSpecF2','RetLoadReason','RetLoadStrgF1','RetLoadTypeF1','RetLoadSpecF1','RetLoadSpecF1','RetLoadStrgF2','RetLoadTypeF2','RetLoadSpecF2','RetLoadSpecF2','RunUnldReason','RunUnldStrgF1','RunUnldTypeF1','RunUnldSpecF1','RunUnldSpecF1','RunUnldStrgF2','RunUnldTypeF2','RunUnldSpecF2','RunUnldSpecF2','RunLoadReason','RunLoadStrgF1','RunLoadTypeF1','RunLoadSpecF1','RunLoadSpecF1','RunLoadStrgF2','RunLoadTypeF2','RunLoadSpecF2','RunLoadSpecF2'],'BOMProductionDetails':['Line','BOM','Variant','ProductionQty','UoM','LocAllLevels','BOMLevel','UseDefaultValue','InventoryValueCriteria','EvaluateByLot','Lot','CostCenter','Job','Notes'],'HKLBOMItem':['Description'],'DBTBOMProductionCompDetails':['Selected','BOM','BreakingBOM','CodeType','Component','ComponentDescription','Qty','UoM','SummaryQty','BaseUoM','ScrapQty','ScrapUM','SummaryWasteQty'],'DBTBOMLotComponentsDetail':['Component','ComponentDescription','UoM','SummaryQty','Lot'],'ProgressViewer':['TEnhProgressViewer_P1','TEnhProgressViewer_P2','TEnhProgressViewer_P3'],'HKLWstSameReason':['Description'],'HKLWstSameStrgF1':['Description'],'HKLWstSameSpecF1':['CompanyName','CompanyName'],'HKLWstSameStrgF2':['Description'],'HKLWstSameSpecF2':['CompanyName','CompanyName'],'HKLWstDiffReason':['Description'],'HKLWstDiffStrgF1':['Description'],'HKLWstDiffSpecF1':['CompanyName','CompanyName'],'HKLWstDiffStrgF2':['Description'],'HKLWstDiffSpecF2':['CompanyName','CompanyName'],'HKLRetUnldReason':['Description'],'HKLRetUnldStrgF1':['Description'],'HKLRetUnldSpecF1':['CompanyName','CompanyName'],'HKLRetUnldStrgF2':['Description'],'HKLRetUnldSpecF2':['CompanyName','CompanyName'],'HKLRetLoadReason':['Description'],'HKLRetLoadStrgF1':['Description'],'HKLRetLoadSpecF1':['CompanyName','CompanyName'],'HKLRetLoadStrgF2':['Description'],'HKLRetLoadSpecF2':['CompanyName','CompanyName'],'HKLRunUnldReason':['Description'],'HKLRunUnldStrgF1':['Description'],'HKLRunUnldSpecF1':['CompanyName','CompanyName'],'HKLRunUnldStrgF2':['Description'],'HKLRunUnldSpecF2':['CompanyName','CompanyName'],'HKLRunLoadReason':['Description'],'HKLRunLoadStrgF1':['Description'],'HKLRunLoadSpecF1':['CompanyName','CompanyName'],'HKLRunLoadStrgF2':['Description'],'HKLRunLoadSpecF2':['CompanyName','CompanyName']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_BOM_PROD_DATAFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_BOM_PROD_DATAComponent, resolver);
    }
} 