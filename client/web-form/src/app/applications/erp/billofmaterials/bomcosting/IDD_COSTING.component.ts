import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_COSTINGService } from './IDD_COSTING.service';

@Component({
    selector: 'tb-IDD_COSTING',
    templateUrl: './IDD_COSTING.component.html',
    providers: [IDD_COSTINGService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_COSTINGComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_COSTING_COMBO_VALUATION_itemSource: any;

    constructor(document: IDD_COSTINGService,
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
        this.IDC_COSTING_COMBO_VALUATION_itemSource = {
  "name": "ValueTypeCombo",
  "namespace": "ERP.Company.Components.ValuationInventoryCombo"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['bBOMSelNone','bBOMSelAll','bBOMSel','FromBOM','ToBOM','bPreferredSel','bAlternateSel','Alternate','bOnlyExactMatchAlt','bNotExplodeItemVariant','bAllVariantItems','bItemVariantSel','FromItem','ToItem','bNotExplodeVariant','bAllVariants','bVariantSel','FromVariant','ToVariant','bApplyECO','ECODate','ECORevision','bLevelSelAll','bLevelSel','NrLevels','EnableLot','QuantityToCost','RoundingValue','bCompRounding','bDateSelAll','bOnlyValidComponentsCosting','Date','bProdParamValueType','bDefaultComponentValueType','bSpecificValueType','ValueType','bEvaluateByLot','ValueType','bEvaluateByLot','ShowResultGrid','bAlsoSemifinished','bAlsoPhantomBOM','ExplodeAll','bAlsoDisabledBOM','SimulationBOMCost','bRecalculate','BOMCosting','BOMCostingDetail','bStdCostMemo','bProdCostMemo'],'BOMCosting':['Bmp','Selected','BreakingItem','ProductDes','BreakingVariant','BreakingBOM','CodeType','Cost','PurchaseCost','SetupCost','InhouseProcessingCost','OutsourcedProcessingCost','Updated'],'BOMCostingDetail':['l_Bmp','BOM','BOMVariant','ProductDes','Component','ComponentVariant','ComponentDescription','PurchaseCost','SetupCost','InhouseProcessingCost','OutsourcedProcessingCost']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_COSTINGFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_COSTINGComponent, resolver);
    }
} 