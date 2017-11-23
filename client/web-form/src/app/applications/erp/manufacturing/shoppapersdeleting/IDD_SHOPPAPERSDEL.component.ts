import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_SHOPPAPERSDELService } from './IDD_SHOPPAPERSDEL.service';

@Component({
    selector: 'tb-IDD_SHOPPAPERSDEL',
    templateUrl: './IDD_SHOPPAPERSDEL.component.html',
    providers: [IDD_SHOPPAPERSDELService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_SHOPPAPERSDELComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_SHOPPAPERSDEL_SELECTION_itemSource: any;
public IDC_SHOPPAPERSDEL_MO_STATUS_itemSource: any;

    constructor(document: IDD_SHOPPAPERSDELService,
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
        this.IDC_SHOPPAPERSDEL_SELECTION_itemSource = {
  "name": "SelectionStrCombo",
  "namespace": "Erp.Manufacturing.Documents.ShopPaperDeletionDocTypeItemSource"
}; 
this.IDC_SHOPPAPERSDEL_MO_STATUS_itemSource = {
  "name": "MOStatusEnumCombo",
  "namespace": "ERP.Manufacturing.Documents.StatusNotTermBoLItemSource"
}; 

        		this.bo.appendToModelStructure({'global':['Selection','bMOMOAll','bMOMOSel','bMOMOFrom','bMOMOTo','bMODateAll','bMODateSel','MODateFrom','MODateTo','bMOJobAll','bMOJobSel','MOJobFrom','MOJobTo','bMOItemAll','bMOItemSel','MOItemFrom','MOItemTo','bMOVariantAll','bMOVariantSel','MOVariantFrom','MOVariantTo','bMOStatusAll','bMOStatusSel','MOStatus','bMODeleteConfirmed','bDeleteSubcntShopPapers','bMORestoreProdPlanLine','bDeleteInvEntries','bDeleteCorrInvEntries','bPlanPlanAll','bPlanPlanSel','PlanPlanFrom','PlanPlanTo','bPlanDateAll','bPlanDateSel','bPlanDateFrom','bPlanDateTo','bJTJTAll','bJTJTSel','bJTJTFrom','bJTJTTo','bJTProdRunAll','bJTProdRunSel','JTProdRunNo','bDeleteSubcntShopPapers','bDeleteInvEntries','bDeleteCorrInvEntries','bPLPLAll','bPLPLSel','bPLPLFrom','bPLPLTo','bDeleteInvEntries','bDeleteCorrInvEntries','DeleteShopPapers','nCurrentElement','GaugeDescription','ProgressViewer'],'DeleteShopPapers':['TMO_Selection','TMO_StateBmp','MONo','DeliveryDate','MOStatus','BOM','TMO_BOMDescri','UoM','Variant','PlanNo','Job','ProductionPlanDescri','PlanCreationDate','ProductionPlanNotes','JobTicketNo','ProductionRunNo','OutsourcedJT','WC','MONo','MORtgStep','PickingListNo','PLCreationDate','MONo'],'ProgressViewer':['TEnhProgressViewer_P1','TEnhProgressViewer_P2','TEnhProgressViewer_P3']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_SHOPPAPERSDELFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_SHOPPAPERSDELComponent, resolver);
    }
} 