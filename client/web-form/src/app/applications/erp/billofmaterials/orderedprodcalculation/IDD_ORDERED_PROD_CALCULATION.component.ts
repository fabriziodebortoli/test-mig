import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_ORDERED_PROD_CALCULATIONService } from './IDD_ORDERED_PROD_CALCULATION.service';

@Component({
    selector: 'tb-IDD_ORDERED_PROD_CALCULATION',
    templateUrl: './IDD_ORDERED_PROD_CALCULATION.component.html',
    providers: [IDD_ORDERED_PROD_CALCULATIONService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_ORDERED_PROD_CALCULATIONComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_ORDERED_PROD_CALCULATIONService,
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
        
        		this.bo.appendToModelStructure({'global':['nCurrentElement','GaugeDescription','BookInventoryMago4','OnHandMago4','ShopFloorStorage','CancelInHouseRMReseInvRsn','RevOutsrcToInHouseInvRsn','RevInHouseRMIssueInvRsn','RevInHouseFPReceiptInvRsn','RevInHouseSRLoadInvRsn','RevInHouseScrapInvRsn','CancelOutsrcRMReseInvRsn','RevOutsrcRMPickingInvRsn','RevOutsrcToOutsrcInvRsn','RevInHouseToOutsrcInvRsn','RevOutsrcRMIssueInvRsn','RevOutsrcFPReceiptInvRsn','RevOutsrcSRLoadInvRsn','RevOutsrcScrapInvRsn','CancelInHouseRMReseInvRsn','RevOutsrcToInHouseInvRsn','RevInHouseRMIssueInvRsn','InHouseFPReceiptInvRsn','RevInHouseScrapInvRsn','CancelOutsrcRMReseInvRsn','RevOutsrcRMPickingInvRsn','RevOutsrcToOutsrcInvRsn','RevInHouseToOutsrcInvRsn','RevOutsrcRMIssueInvRsn','RevOutsrcFPReceiptInvRsn','RevOutsrcScrapInvRsn','InHouseRMReservInvRsn','OutsrcToInHouseInvRsn','InHouseRMIssueInvRsn','InHouseFPReceiptInvRsn','InHouseSRLoadInvRsn','InHouseScrapInvRsn','InHouseStdSFReservInvRsn','OutsrcRMReservInvRsn','OutsrcRMPickingInvRsn','OutsrcToOutsrcInvRsn','InHouseToOutsrcInvRsn','OutsrcRMIssueInvRsn','OutsrcFPReceiptInvRsn','OutsrcSRLoadInvRsn','OutsrcScrapInvRsn','OutsrcStdSFReservInvRsn','InHouseRMReservInvRsn','OutsrcToInHouseInvRsn','InHouseRMIssueInvRsn','InHouseFPReceiptInvRsn','InHouseScrapInvRsn','InHouseStdSFReservInvRsn','OutsrcRMReservInvRsn','OutsrcRMPickingInvRsn','OutsrcToOutsrcInvRsn','InHouseToOutsrcInvRsn','OutsrcRMIssueInvRsn','OutsrcFPReceiptInvRsn','OutsrcScrapInvRsn','OutsrcStdSFReservInvRsn']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_ORDERED_PROD_CALCULATIONFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_ORDERED_PROD_CALCULATIONComponent, resolver);
    }
} 