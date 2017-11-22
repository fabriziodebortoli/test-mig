import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_ACCTRANSFERService } from './IDD_ACCTRANSFER.service';

@Component({
    selector: 'tb-IDD_ACCTRANSFER',
    templateUrl: './IDD_ACCTRANSFER.component.html',
    providers: [IDD_ACCTRANSFERService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_ACCTRANSFERComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_ACCTRANSFERService,
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
		boService.appendToModelStructure({'global':['StartDate','EndDate','PostingDate','AccrualDate','Detail'],'Detail':['l_TEnhAccTransfer_P01','l_TEnhAccTransfer_P02','l_TEnhAccTransfer_P04','l_TEnhAccTransfer_P03','l_TEnhAccTransfer_P06','Account','l_TEnhAccTransfer_P05','DebitCreditSign','Amount']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_ACCTRANSFERFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_ACCTRANSFERComponent, resolver);
    }
} 