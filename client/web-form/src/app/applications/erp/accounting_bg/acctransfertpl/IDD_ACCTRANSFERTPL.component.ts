import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_ACCTRANSFERTPLService } from './IDD_ACCTRANSFERTPL.service';

@Component({
    selector: 'tb-IDD_ACCTRANSFERTPL',
    templateUrl: './IDD_ACCTRANSFERTPL.component.html',
    providers: [IDD_ACCTRANSFERTPLService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_ACCTRANSFERTPLComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_ACCTRANSFERTPLService,
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
		boService.appendToModelStructure({'AccTransferTpl':['Template','Description','Priority','ValidityStartingDate','ValidityEndingDate'],'global':['AccTransferTplOrigin','AccTransferTplDest','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'AccTransferTplOrigin':['Account','BalanceSide','BalanceType','IgnoreDifferentSign'],'HKLOriginAccount':['Description'],'AccTransferTplDest':['Account','TransferPerc'],'HKLDestAccount':['Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_ACCTRANSFERTPLFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_ACCTRANSFERTPLComponent, resolver);
    }
} 