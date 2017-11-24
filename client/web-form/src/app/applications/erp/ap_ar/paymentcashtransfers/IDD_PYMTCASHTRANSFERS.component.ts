import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_PYMTCASHTRANSFERSService } from './IDD_PYMTCASHTRANSFERS.service';

@Component({
    selector: 'tb-IDD_PYMTCASHTRANSFERS',
    templateUrl: './IDD_PYMTCASHTRANSFERS.component.html',
    providers: [IDD_PYMTCASHTRANSFERSService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_PYMTCASHTRANSFERSComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_PYMTCASHTRANSFERSService,
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
        
        		this.bo.appendToModelStructure({'PaymentCashTransfers':['TransferDate','TransferAmount','SourcePymtCash','DestinationPymtCash','Notes'],'HKLPymtSourceCash':['Description'],'HKLPymtDestinationCash':['Description'],'global':['ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_PYMTCASHTRANSFERSFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_PYMTCASHTRANSFERSComponent, resolver);
    }
} 