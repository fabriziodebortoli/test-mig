import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_MRP_SIMService } from './IDD_MRP_SIM.service';

@Component({
    selector: 'tb-IDD_MRP_SIM',
    templateUrl: './IDD_MRP_SIM.component.html',
    providers: [IDD_MRP_SIMService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_MRP_SIMComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_MRP_SIM_INPUT_LT_FINISHED_itemSource: any;
public IDC_MRP_SIM_INPUT_LT_PURC_itemSource: any;

    constructor(document: IDD_MRP_SIMService,
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
        this.IDC_MRP_SIM_INPUT_LT_FINISHED_itemSource = {
  "name": "ForFinished",
  "namespace": "ERP.MRP.Documents.LeadTimeOrginItemSource"
}; 
this.IDC_MRP_SIM_INPUT_LT_PURC_itemSource = {
  "name": "ForPurchase",
  "namespace": "ERP.MRP.Documents.LeadTimeOrginItemSource"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'DBTMRPSimulation':['Simulation','Description','RunDate','FromProdPlan','FromSaleOrd','FromMOExisting','FromReorderPoint','ReorderPointDate','PunctualReorderPoint','FinishedLeadTimeOrigin','PurchaseLeadTimeOrigin','ProdPlan','SaleOrdersOnly','IncludeSaleOrdExpired','HrzType','Horizon','HrzEndDate','AllPriorities','SelectPriority','FromPriority','ToPriority','GroupByJob','GroupByJobDate','GroupByJobGroupSF','SaleOrdersOnly','IncludeSaleOrdExpired','HrzType','Horizon','HrzEndDate','AllPriorities','SelectPriority','ToPriority','FromPriority','OpenOrdersOnly','SaleForecastsOnly','SaleOrdSaleForeRelation','DayOfRequirement','WeekDayOfRequirement','AllMO','SelectMO','FromMO','ToMO','MOCreated','MOReleased','MOInProgress','NetFirstLevelOnDayReqPolicy','NetOtherLevelsOnDayReqPolicy','NetFirstLevelOnJobPolicy','NetOtherLevelsOnJobPolicy','NetFirstLevelOnLotPolicy','NetOtherLevelsOnLotPolicy','NetByItemMaster','SelectNetSetting','NetByMO','NetByPurchOrd','NetByMOConfirmed','NetByMoEmptyJob','UseSimulatedEndDate','UseAheadDepositMO','AheadDepositDaysMO','UseDeliveryExpectedDatePO','UseDeliveryConfirmedDatePO','UseAheadDepositPO','AheadDepositDaysPO','OnlyPurchOrdWithMrpStorage','UseCustOrdDeliveryDate','UseCustOrdConfirmDate','ExplodeAllBOMLevels','SelectBOMLevel','MaxLevelBOMExplosion','CreateMOOnly','CreatePROnly','GroupLotsByDate','SkipNotWorkingDays','OrderReleaseDays','AnticipationDays','UseMinStock','MovePastDate','Signature','Notes'],'HKLProductionPlan':['Description'],'global':['HFItems_All','HFItems_Range','HFItems_From','HFItems_To','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_MRP_SIMFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_MRP_SIMComponent, resolver);
    }
} 