import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_MRPService } from './IDD_MRP.service';

@Component({
    selector: 'tb-IDD_MRP',
    templateUrl: './IDD_MRP.component.html',
    providers: [IDD_MRPService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_MRPComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_MRP_HORIZON_TYPE_itemSource: any;
public IDC_MRP_FINISHED_LEAD_TIME_itemSource: any;
public IDC_MRP_PURCHASE_LEAD_TIME_itemSource: any;

    constructor(document: IDD_MRPService,
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
        this.IDC_MRP_HORIZON_TYPE_itemSource = {
  "name": "HorizontCombo",
  "namespace": "ERP.MRP.Documents.EnumHorizonItemSource"
}; 
this.IDC_MRP_FINISHED_LEAD_TIME_itemSource = {
  "name": "ForFinished",
  "namespace": "ERP.MRP.Documents.LeadTimeOrginItemSource"
}; 
this.IDC_MRP_PURCHASE_LEAD_TIME_itemSource = {
  "name": "ForPurchase",
  "namespace": "ERP.MRP.Documents.LeadTimeOrginItemSource"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'DBTMRP':['ProdPlan','SaleOrdersOnly','HrzType','HrzEndDate','Horizon','ExplodeAllBOMLevels','SelectBOMLevel','MaxLevelBOMExplosion','FinishedLeadTimeOrigin','PurchaseLeadTimeOrigin','NetFirstLevelOnJobPolicy','NetOtherLevelsOnJobPolicy','NetFirstLevelOnLotPolicy','NetOtherLevelsOnLotPolicy','NetFirstLevelOnDayReqPolicy','NetOtherLevelsOnDayReqPolicy','UseMinStock','SkipNotWorkingDays','GroupLotsByDate','UseSimulatedEndDate'],'HKLProductionPlan':['Description'],'global':['HFItems_All','HFItems_Range','HFItems_From','HFItems_To','DBTResult','nCurrentElement','GaugeDescription','ProgressViewer'],'DBTResult':['Selection','Item','Description','Nature','BOM','Variant','BaseUoM'],'ProgressViewer':['TEnhProgressViewer_P1','TEnhProgressViewer_P2','TEnhProgressViewer_P3']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_MRPFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_MRPComponent, resolver);
    }
} 