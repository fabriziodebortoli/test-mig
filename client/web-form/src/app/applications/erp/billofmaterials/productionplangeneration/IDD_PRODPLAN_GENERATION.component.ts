import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_PRODPLAN_GENERATIONService } from './IDD_PRODPLAN_GENERATION.service';

@Component({
    selector: 'tb-IDD_PRODPLAN_GENERATION',
    templateUrl: './IDD_PRODPLAN_GENERATION.component.html',
    providers: [IDD_PRODPLAN_GENERATIONService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_PRODPLAN_GENERATIONComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_PRODPLAN_GENERATIONService,
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
		boService.appendToModelStructure({'global':['HFCustomer_From','HFCustomer_To','HFCustomer_All','HFCustomer_Range','HFCustomer_From','HFVariant_From','HFVariant_To','HFVariant_All','HFVariant_Range','HFVariant_From','HFJobs_All','HFJobs_Range','HFJobs_From','HFJobs_To','HFCostCenter_All','HFCostCenter_Range','HFCostCenter_From','HFCostCenter_To','HFSaleOrder_All','HFSaleOrder_Range','HFSaleOrder_From','HFSaleOrder_To','HFPriority_From','HFPriority_To','HFPriority_All','HFPriority_Range','HFPriority_From','AllDateOrd','UseConfirmedDeliveryDate','UseExpectedDeliveryDate','StartingDate','EndingDate','GenInvSel','StorageSel','Storage','SpecificatorType','Specificator','GroupByItem','ItemsShortage','InventoryNetting','Shortage','Insufficiency','BOMLevel','AlsoItem','IssueComponent','HFItems_All','HFItems_Range','HFItems_From','HFItems_To','ProductionPlanGeneration'],'ProductionPlanGeneration':['Selected','Item','Variant','Description','BaseUoM','UoM','Production','ReferenceDocNo','Position']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_PRODPLAN_GENERATIONFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_PRODPLAN_GENERATIONComponent, resolver);
    }
} 