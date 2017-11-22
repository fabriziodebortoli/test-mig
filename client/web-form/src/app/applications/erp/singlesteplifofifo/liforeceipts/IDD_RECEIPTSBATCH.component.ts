import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_RECEIPTSBATCHService } from './IDD_RECEIPTSBATCH.service';

@Component({
    selector: 'tb-IDD_RECEIPTSBATCH',
    templateUrl: './IDD_RECEIPTSBATCH.component.html',
    providers: [IDD_RECEIPTSBATCHService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_RECEIPTSBATCHComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_RECEIPTSBATCHService,
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
        
        const boService = this.document as BOService;
		boService.appendToModelStructure({'Receipts':['Item','ReceiptBatchId','Storage','LoadDate','TotallyConsumedDate','AccountingType','UnitValue','Qty','ReceiptBatchValue'],'HKLItems':['BaseUoM','BaseUoM','Description'],'HKLAccountingType':['Description'],'global':['ReceiptDetails','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'ReceiptDetails':['InvEntryType','PostingDate','Reason','InvEntryLine','QtyIn','ValueIn','QtyOut','ValueOut','QtyToDate','ValueToDate','LineCost','LoadUnitValue']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_RECEIPTSBATCHFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_RECEIPTSBATCHComponent, resolver);
    }
} 