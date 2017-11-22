import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_BOMCOSTSIMULATIONSService } from './IDD_BOMCOSTSIMULATIONS.service';

@Component({
    selector: 'tb-IDD_BOMCOSTSIMULATIONS',
    templateUrl: './IDD_BOMCOSTSIMULATIONS.component.html',
    providers: [IDD_BOMCOSTSIMULATIONSService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_BOMCOSTSIMULATIONSComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_COSTING_COMBO_VALUATION_itemSource: any;

    constructor(document: IDD_BOMCOSTSIMULATIONSService,
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
		boService.appendToModelStructure({'BOMCostSimulations':['SimulationBOMCost','UpdateCostDate'],'global':['bBOMSelNone','bBOMSelAll','bBOMSel','FromBOM','ToBOM','bPreferredSel','bAlternateSel','Alternate','bOnlyExactMatchAlt','bNotExplodeItemVariant','bAllVariantItems','bItemVariantSel','FromItem','ToItem','bNotExplodeVariant','bAllVariants','bVariantSel','FromVariant','ToVariant','bApplyECO','ECODate','ECORevision','bLevelSelAll','bLevelSel','NrLevels','EnableLot','QuantityToCost','RoundingValue','bCompRounding','bDateSelAll','bOnlyValidComponentsCosting','Date','bProdParamValueType','bDefaultComponentValueType','bSpecificValueType','ValueType','bEvaluateByLot','ValueType','bEvaluateByLot','ShowResultGrid','bAlsoSemifinished','bAlsoPhantomBOM','ExplodeAll','bAlsoDisabledBOM','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_BOMCOSTSIMULATIONSFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_BOMCOSTSIMULATIONSComponent, resolver);
    }
} 