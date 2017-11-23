import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_COSTTRANSFERService } from './IDD_COSTTRANSFER.service';

@Component({
    selector: 'tb-IDD_COSTTRANSFER',
    templateUrl: './IDD_COSTTRANSFER.component.html',
    providers: [IDD_COSTTRANSFERService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_COSTTRANSFERComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_COSTTRANSFERService,
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
		boService.appendToModelStructure({'global':['StartMonth','StartYear','EndMonth','EndYear','bBudget','bActual','bForecast','PostingDate','AccrualDate','Detail'],'Detail':['l_TEnhTransfer_P01','l_TEnhTransfer_P02','l_TEnhTransfer_P04','l_TEnhTransfer_P03','l_TEnhTransfer_P08','l_TEnhTransfer_P05','l_TEnhTransfer_P06','l_TEnhTransfer_P12','l_TEnhTransfer_P11','DebitCreditSign','TotalAmount']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_COSTTRANSFERFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_COSTTRANSFERComponent, resolver);
    }
} 