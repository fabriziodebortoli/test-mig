import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_MRPWIZService } from './IDD_MRPWIZ.service';

@Component({
    selector: 'tb-IDD_MRPWIZ',
    templateUrl: './IDD_MRPWIZ.component.html',
    providers: [IDD_MRPWIZService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_MRPWIZComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_MRP_SIM_INPUT_LT_FINISHED_itemSource: any;
public IDC_MRP_SIM_INPUT_LT_PURC_itemSource: any;

    constructor(document: IDD_MRPWIZService,
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
        this.IDC_MRP_SIM_INPUT_LT_FINISHED_itemSource = {
  "name": "ForFinished",
  "namespace": "ERP.MRP.Documents.LeadTimeOrginItemSource"
}; 
this.IDC_MRP_SIM_INPUT_LT_PURC_itemSource = {
  "name": "ForPurchase",
  "namespace": "ERP.MRP.Documents.LeadTimeOrginItemSource"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'DBTMRPSimulation':['Simulation','Description','RunDate','FromProdPlan','FromSaleOrd','FromMOExisting','FromReorderPoint','ReorderPointDate','PunctualReorderPoint','FinishedLeadTimeOrigin','PurchaseLeadTimeOrigin','ProdPlan','SaleOrdersOnly','IncludeSaleOrdExpired','HrzType','Horizon','HrzEndDate','AllPriorities','SelectPriority','FromPriority','ToPriority','GroupByJob','GroupByJobDate','GroupByJobGroupSF','SaleOrdersOnly','IncludeSaleOrdExpired','HrzType','Horizon','HrzEndDate','AllPriorities','SelectPriority','ToPriority','FromPriority','OpenOrdersOnly','SaleForecastsOnly','SaleOrdSaleForeRelation','DayOfRequirement','WeekDayOfRequirement','AllMO','SelectMO','FromMO','ToMO','MOCreated','MOReleased','MOInProgress','NetFirstLevelOnDayReqPolicy','NetOtherLevelsOnDayReqPolicy','NetFirstLevelOnJobPolicy','NetOtherLevelsOnJobPolicy','NetFirstLevelOnLotPolicy','NetOtherLevelsOnLotPolicy','NetByItemMaster','SelectNetSetting','NetByMO','NetByPurchOrd','NetByMOConfirmed','NetByMoEmptyJob','UseSimulatedEndDate','UseAheadDepositMO','AheadDepositDaysMO','UseDeliveryExpectedDatePO','UseDeliveryConfirmedDatePO','UseAheadDepositPO','AheadDepositDaysPO','OnlyPurchOrdWithMrpStorage','UseCustOrdDeliveryDate','UseCustOrdConfirmDate','ExplodeAllBOMLevels','SelectBOMLevel','MaxLevelBOMExplosion','CreateMOOnly','CreatePROnly','GroupLotsByDate','SkipNotWorkingDays','OrderReleaseDays','AnticipationDays','UseMinStock','MovePastDate','Signature','Notes'],'HKLProductionPlan':['Description'],'global':['HFItems_All','HFItems_Range','HFItems_From','HFItems_To','DBTResults','nCurrentElement','GaugeDescription','ProgressViewer'],'DBTResults':['Selection','Item','Description','Nature','BOM','BaseUoM','ProductionQty'],'ProgressViewer':['TEnhProgressViewer_P1','TEnhProgressViewer_P2','TEnhProgressViewer_P3']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_MRPWIZFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_MRPWIZComponent, resolver);
    }
} 