import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_ITEMS_WAP_TRANSACTIONS_DELETINGService } from './IDD_ITEMS_WAP_TRANSACTIONS_DELETING.service';

@Component({
    selector: 'tb-IDD_ITEMS_WAP_TRANSACTIONS_DELETING',
    templateUrl: './IDD_ITEMS_WAP_TRANSACTIONS_DELETING.component.html',
    providers: [IDD_ITEMS_WAP_TRANSACTIONS_DELETINGService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_ITEMS_WAP_TRANSACTIONS_DELETINGComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_ITEMS_WAP_TRANSACTIONS_DELETINGService,
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
        
        		this.bo.appendToModelStructure({'global':['HFPeriod_From','HFPeriod_To','HFItems_All','HFItems_Range','HFItems_From','HFItems_To','DBTSummaryDetail'],'DBTSummaryDetail':['l_LineSummaryDescription']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_ITEMS_WAP_TRANSACTIONS_DELETINGFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_ITEMS_WAP_TRANSACTIONS_DELETINGComponent, resolver);
    }
} 