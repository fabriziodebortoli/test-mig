import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_SUBSBOMITEMService } from './IDD_SUBSBOMITEM.service';

@Component({
    selector: 'tb-IDD_SUBSBOMITEM',
    templateUrl: './IDD_SUBSBOMITEM.component.html',
    providers: [IDD_SUBSBOMITEMService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_SUBSBOMITEMComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_SUBSCOMPDB_OLD_COMPONENTTYPE_itemSource: any;
public IDC_SUBSCOMPDB_NEW_COMPONENTTYPE_itemSource: any;
public IDC_SUBSCOMPDB_NEW_UOM_itemSource: any;

    constructor(document: IDD_SUBSBOMITEMService,
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
        this.IDC_SUBSCOMPDB_OLD_COMPONENTTYPE_itemSource = {
  "name": "CodeTypeEnumCombo",
  "namespace": "ERP.BillOfMaterials.Components.TypeBillOfMaterialsItemSource"
}; 
this.IDC_SUBSCOMPDB_NEW_COMPONENTTYPE_itemSource = {
  "name": "CodeTypeEnumCombo",
  "namespace": "ERP.BillOfMaterials.Components.TypeBillOfMaterialsItemSource"
}; 
this.IDC_SUBSCOMPDB_NEW_UOM_itemSource = {
  "name": "UnitsOfMeasureCompNewComboBox",
  "namespace": "ERP.BillOfMaterials.Documents.UnitsOfMeasureCompNewComboBox"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['bBOMCompReplace','bBOMCompInsert','bBOMCompDelete','bBOMSelAll','bBOM','BOM','bBOMVariant_SelBOM_NoVar','bBOMVariant_SelBOM_Var','BOMVariant','bBOMVariant_SelBOM_AllVar','bCompAllPeriod','CompValidFrom','CompValidTo','OldComponentNature','OldComponent','NewComponentNature','NewComponent','bAllVariantsCompOld','OldVariant','NewVariant','OldUoM','OldQty','NewUoM','NewQty','bAllPeriod','ValidFrom','ValidTo','bRecursiveCtrl','BOMComponentsReplacement'],'BOMComponentsReplacement':['Selected','BOM','Item','BOMVariant','Component','ComponentVariant','Qty','PercQty','UoM','NewComponent','NewVariant','NewQty','NewQtyReplaced','PercQty','TotPercBmp','NewUoM','ValidityStartingDate','ValidityEndingDate','CompVarVariationType','ECOAutGen','ECONo'],'HKLBillOfMaterials':['Description'],'HKLItems':['Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_SUBSBOMITEMFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_SUBSBOMITEMComponent, resolver);
    }
} 