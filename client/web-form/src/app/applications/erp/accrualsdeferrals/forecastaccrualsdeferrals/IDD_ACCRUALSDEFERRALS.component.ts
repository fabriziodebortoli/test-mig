import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_ACCRUALSDEFERRALSService } from './IDD_ACCRUALSDEFERRALS.service';

@Component({
    selector: 'tb-IDD_ACCRUALSDEFERRALS',
    templateUrl: './IDD_ACCRUALSDEFERRALS.component.html',
    providers: [IDD_ACCRUALSDEFERRALSService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_ACCRUALSDEFERRALSComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_ACCRUALSDEFERRALSService,
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
        
        		this.bo.appendToModelStructure({'global':['FromDate','ToDate','OrderByAccount','AlreadyAcc','AllEntry','Nature','PostingDate','AccrualDate','SimulationCode','IntialDeferrals','IntialAccruals','OpeningPostingDate','OpeningAccrualDate','Detail'],'Detail':['l_TEnhAccrualsDeferrals_P01','l_TEnhAccrualsDeferrals_P13','AccrualDate','l_TEnhAccrualsDeferrals_P16','l_TEnhAccrualsDeferrals_P17','l_TEnhAccrualsDeferrals_P18','l_TEnhAccrualsDeferrals_P19','l_TEnhAccrualsDeferrals_P20','CustSuppType','CustSupp','l_TEnhAccrualsDeferrals_P21','l_TEnhAccrualsDeferrals_P09','l_TEnhAccrualsDeferrals_P03','StartingOfUseDate','EndingOfUseDate','l_TEnhAccrualsDeferrals_P05','l_TEnhAccrualsDeferrals_P07','l_TEnhAccrualsDeferrals_P06','l_TEnhAccrualsDeferrals_P08','AccrualDeferralTot','l_TEnhAccrualsDeferrals_P04','l_TEnhAccrualsDeferrals_P10','l_TEnhAccrualsDeferrals_P11','Nature']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_ACCRUALSDEFERRALSFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_ACCRUALSDEFERRALSComponent, resolver);
    }
} 