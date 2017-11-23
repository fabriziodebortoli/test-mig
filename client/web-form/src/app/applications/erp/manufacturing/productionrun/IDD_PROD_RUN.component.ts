import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_PROD_RUNService } from './IDD_PROD_RUN.service';

@Component({
    selector: 'tb-IDD_PROD_RUN',
    templateUrl: './IDD_PROD_RUN.component.html',
    providers: [IDD_PROD_RUNService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_PROD_RUNComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_PROD_RUNService,
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
        
        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['bAllDates','DateTo','bAllMO','bOrdSel','FromOrd','ToOrd','bAllDeliveryDates','bSelData','FromMODate','ToMODate','bRunDateAll','bRunDateSel','dRunDateFrom','dRunDateTo','bAllJobs','bJobSel','FromJob','ToJob','HFCostCenters_All','HFCostCenters_Range','HFCostCenters_From','HFCostCenters_To','bAllProd','bProdSel','FromProd','ToProd','bAllVariants','bVariantsSel','FromVariant','ToVariant','ChooseAlternateAuto','ChoosePreferredAlternate','ChooseCodeAlternate','AlternateCode','ChooseAlternateManual','bOnlyExactMatch','bAllMRPRanks','bMRPRanksSel','FromMRPRank','ToMRPRank','ChooseAlternate','PhaseDetail','bInHouseStepsSearchProductionStorages','bInHouseStepsSearchDefaultStorage','bOutsourcedStepsSearchProductionStorages','bOutsourcedStepsSearchDefaultStorage','bOutsourcedStepsSearchDefaultSubnctStorage','bOutsourcedStepsSearchSubcntStorage','bCreateSubcntOrd','bSeparateOrderForMO','bCreateSubcntDN','bOutsourcedStepsTakeStorageLotsFromDN','JobTicketType','Storage','StorageSemifinished','bPickMOComponents','bPickRequiredMaterials','bOverloadTool','nCurrentElement','GaugeDescription','ProgressViewer','InCreatedPic','InReleasedPic','InProcessingPic','OutCreatedPic','OutReleasedPic','OutProcessingPic'],'ChooseAlternate':['Selection','MONo','RtgStep','BOM','Variant','Alternate','AltRtgStep','Operation','WC'],'PhaseDetail':['StateBmp','Selection','MONo','WC','RtgStep','Alternate','AltRtgStep','BOM','Variant','Operation','Supplier','ProductionQty','UoM','BOMDescri','StepDeliveryDate','Lot'],'ProgressViewer':['TEnhProgressViewer_P1','TEnhProgressViewer_P2','TEnhProgressViewer_P3']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_PROD_RUNFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_PROD_RUNComponent, resolver);
    }
} 