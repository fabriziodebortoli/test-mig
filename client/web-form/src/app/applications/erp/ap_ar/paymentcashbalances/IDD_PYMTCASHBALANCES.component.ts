import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_PYMTCASHBALANCESService } from './IDD_PYMTCASHBALANCES.service';

@Component({
    selector: 'tb-IDD_PYMTCASHBALANCES',
    templateUrl: './IDD_PYMTCASHBALANCES.component.html',
    providers: [IDD_PYMTCASHBALANCESService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_PYMTCASHBALANCESComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_PYMTCASHBALANCESService,
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
		boService.appendToModelStructure({'PaymentCashBalances':['PymtCash','BalanceDate','BalanceAmount'],'HKLPymtCash':['Description'],'global':['ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_PYMTCASHBALANCESFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_PYMTCASHBALANCESComponent, resolver);
    }
} 