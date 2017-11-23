import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_LOTS_SUPP_ADD_ON_FLYService } from './IDD_LOTS_SUPP_ADD_ON_FLY.service';

@Component({
    selector: 'tb-IDD_LOTS_SUPP_ADD_ON_FLY',
    templateUrl: './IDD_LOTS_SUPP_ADD_ON_FLY.component.html',
    providers: [IDD_LOTS_SUPP_ADD_ON_FLYService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_LOTS_SUPP_ADD_ON_FLYComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_LOTS_SUPP_ADD_ON_FLYService,
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
		boService.appendToModelStructure({'LotsAddOnFly':['Lot','Disabled','Description','ValidFrom','ValidTo','MinimumStock','TotallyConsumed','OutOfStockDate','InternallyProduced','BarcodeSegment','Notes','LoadDate','Storage','Supplier','PurchaseOrdNo','NoOfPacks','ReceiptDocNo','SupplierLotNo','ParentLotNo','AnalysisRefNo','AnalysisPerson','AnalysisDate','AnalysisStatus'],'HKLStorage':['Description'],'HKLSupplier':['CompNameComplete'],'global':['ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_LOTS_SUPP_ADD_ON_FLYFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_LOTS_SUPP_ADD_ON_FLYComponent, resolver);
    }
} 