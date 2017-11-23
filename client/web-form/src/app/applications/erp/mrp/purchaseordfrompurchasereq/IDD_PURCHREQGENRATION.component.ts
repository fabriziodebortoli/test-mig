import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_PURCHREQGENRATIONService } from './IDD_PURCHREQGENRATION.service';

@Component({
    selector: 'tb-IDD_PURCHREQGENRATION',
    templateUrl: './IDD_PURCHREQGENRATION.component.html',
    providers: [IDD_PURCHREQGENRATIONService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_PURCHREQGENRATIONComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_PURCHREQGENRATIONService,
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
		boService.appendToModelStructure({'global':['HFMRPSimulation_MRPSimulation','HFMRPSimulation_MRPSimulationDesc','HFMRPSimulation_AlsoManualPR','HFPRIssueDate_All','HFPRIssueDate_Range','HFPRIssueDate_From','HFPRIssueDate_To','HFPRDeliveryDate_All','HFPRDeliveryDate_Range','HFPRDeliveryDate_From','HFPRDeliveryDate_To','HFRank_All','HFRank_Range','HFRank_From','HFRank_To','HFMRPSupplier_All','HFMRPSupplier_Range','HFMRPSupplier_From','HFMRPSupplier_To','HFMRPSupplier_AlsoEmpty','HFMRPItem_From','HFMRPItem_To','HFMRPItem_All','HFMRPItem_Range','HFMRPItem_From','HFMRPVariant_From','HFMRPVariant_To','HFMRPVariant_All','HFMRPVariant_Range','HFMRPVariant_From','HFMRPJob_All','HFMRPJob_Range','HFMRPJob_From','HFMRPJob_To','HFMRPJob_AlsoEmpty','PurcReqGeneration','bCreateAPurchaseOrdForSupplier','bKeepProposedDate','bGenerateLot','bDeleteRows','bKeepPrice','bUpdatePrice'],'PurcReqGeneration':['Selected','MRPConfirmationRank','Supplier','SupplierDescri','Item','Variant','ItemDescription','ConfirmationLevelDescr','IssueDate','DeliveryDate','UoM','Qty','UnitValue','DiscountFormula','Currency','Job','CostCenter','PurchaseReqNo','Position','ShipToCustSuppType','ShipToCustSupp','GoodsDelivery','DeliveryGoodDescri','Storage'],'HKLJob':['Description'],'HKLCostCnt':['Description'],'HKLStorageForProd':['Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_PURCHREQGENRATIONFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_PURCHREQGENRATIONComponent, resolver);
    }
} 