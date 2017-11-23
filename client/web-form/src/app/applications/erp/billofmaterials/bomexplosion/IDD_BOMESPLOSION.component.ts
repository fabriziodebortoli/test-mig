import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_BOMESPLOSIONService } from './IDD_BOMESPLOSION.service';

@Component({
    selector: 'tb-IDD_BOMESPLOSION',
    templateUrl: './IDD_BOMESPLOSION.component.html',
    providers: [IDD_BOMESPLOSIONService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_BOMESPLOSIONComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_ESPLOS_CODETYPE_BOM_FOR_CODETYPE_itemSource: any;
public IDC_ESPLOS_CODETYPE_BOM_SEL_itemSource: any;

    constructor(document: IDD_BOMESPLOSIONService,
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
        this.IDC_ESPLOS_CODETYPE_BOM_FOR_CODETYPE_itemSource = {
  "name": "CTypeBillOfMaterialsEnumCombo",
  "namespace": "ERP.BillOfMaterials.Components.TypeBillOfMaterialsItemSource"
}; 
this.IDC_ESPLOS_CODETYPE_BOM_SEL_itemSource = {
  "name": "CTypeBillOfMaterialsEnumCombo",
  "namespace": "ERP.BillOfMaterials.Components.TypeBillOfMaterialsItemSource"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['bBOMSelAll','bBOMTypeSel','BOMTypeByType','bBOMSel','BOMTypeSel','FromBOM','ToBOM','bBOMSelNone','bAllItemVariants','bItemVariantSel','FromItem','ToItem','bNotExplodeItemVariant','bAllVariants','bSelVariant','FromVariant','ToVariant','bNotExplodeVariant','bRoutingsExplosion','bAlsoSemifinished','bAlsoDisabledBOM','bApplyECO','ECODate','ECORevision','bLevelSelAll','bLevelSel','NrLevels','bDateSelAll','bSelData','Date','DBTSummaryDetail'],'DBTSummaryDetail':['l_LineSummaryDescription']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_BOMESPLOSIONFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_BOMESPLOSIONComponent, resolver);
    }
} 