import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_CRPService } from './IDD_CRP.service';

@Component({
    selector: 'tb-IDD_CRP',
    templateUrl: './IDD_CRP.component.html',
    providers: [IDD_CRPService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_CRPComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_CRPService,
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
		boService.appendToModelStructure({'global':['nHorizon','dRunDate','bMaintainModify','QueueTimeOrigin','nQueueHours','bStocksTrend','bDurationsFromMO','bAlsoSimulatedMO','bOnlyMONotSim','bOnlyMRPMO','sSimulation','bRoutingChoose','eCriteriaRoutingStep','bChooseForEveryStep','eCriteriaAlternateRoutingStep','sCustomAlternate','eWCFamCriterion','HFMOJob_All','HFMOJob_Range','HFMOJob_From','HFMOJob_To','HFMOJob_AlsoEmpty','HFRank_All','HFRank_Range','HFRank_From','HFRank_To','HFCustomer_From','HFCustomer_To','HFCustomer_All','HFCustomer_Range','HFCustomer_From','bMOAll','bMORange','sMOFrom','sMOTo','eOrderStatus','HFMODeliveryDate_All','HFMODeliveryDate_Range','HFMODeliveryDate_From','HFMODeliveryDate_To','HFItemByKind_All','HFItemByKind_Range','HFItemByKind_From','HFItemByKind_To','HFItemByKind_Status','HFItemByKind_MakeOrBuy','HFVariant_All','HFVariant_Range','HFVariant_From','HFVariant_To','CRP','bDeleteSim'],'CRP':['TMO_Selection','MONo','TMO_ConfirmationLevelDescr','MRPConfirmationRank','BOM','Variant','TMO_BOMDescri','RunDate','DeliveryDate','ProductionQty','MOStatus','Job','Customer']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_CRPFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_CRPComponent, resolver);
    }
} 