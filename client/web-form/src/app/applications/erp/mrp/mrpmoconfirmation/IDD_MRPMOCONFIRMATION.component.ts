import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_MRPMOCONFIRMATIONService } from './IDD_MRPMOCONFIRMATION.service';

@Component({
    selector: 'tb-IDD_MRPMOCONFIRMATION',
    templateUrl: './IDD_MRPMOCONFIRMATION.component.html',
    providers: [IDD_MRPMOCONFIRMATIONService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_MRPMOCONFIRMATIONComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_MRPMOCONFIRMATIONService,
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
		boService.appendToModelStructure({'global':['HFMRPSimulation_AlsoManualPR','HFMRPSimulation_MRPSimulation','HFMRPSimulation_MRPSimulationDesc','HFMO_From','HFMO_To','HFMO_All','HFMO_Range','HFMO_From','HFMODeliveryDate_All','HFMODeliveryDate_Range','HFMODeliveryDate_From','HFMODeliveryDate_To','HFItemByKind_All','HFItemByKind_Range','HFItemByKind_From','HFItemByKind_To','HFItemByKind_Status','HFItemByKind_MakeOrBuy','HFVariant_All','HFVariant_Range','HFVariant_From','HFVariant_To','HFMRPJob_All','HFMRPJob_Range','HFMRPJob_From','HFMRPJob_To','HFMRPJob_AlsoEmpty','HFRank_All','HFRank_Range','HFRank_From','HFRank_To','HFCustomer_From','HFCustomer_To','HFCustomer_All','HFCustomer_Range','HFCustomer_From','MRPMOConfirmation','bDeleteSim','bDeleteMO','bCreateAPurchaseOrdForSupplier','bKeepProposedDate','bGenerateLot'],'MRPMOConfirmation':['MakeOrBuy','TMO_Selection','MONo','TMO_ConfirmationLevelDescr','MRPConfirmationRank','BOM','Variant','TMO_BOMDescri','RunDate','DeliveryDate','ProductionQty','MOStatus','Job','CostCenter','Customer','TMO_Supplier','SupplierDescri','IssueDate','RequestedDeliveryDate','UnitValue','DiscountFormula','Currency','ShipToCustSuppType','ShipToCustSupp','GoodsDelivery','GoodsDeliveryDesc','Storage'],'HKLJob':['Description'],'HKLCostCnt':['Description'],'HKLStorage':['Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_MRPMOCONFIRMATIONFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_MRPMOCONFIRMATIONComponent, resolver);
    }
} 