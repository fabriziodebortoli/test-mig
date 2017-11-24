import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_TAXSETTLEMENTUPDATEService } from './IDD_TAXSETTLEMENTUPDATE.service';

@Component({
    selector: 'tb-IDD_TAXSETTLEMENTUPDATE',
    templateUrl: './IDD_TAXSETTLEMENTUPDATE.component.html',
    providers: [IDD_TAXSETTLEMENTUPDATEService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_TAXSETTLEMENTUPDATEComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_TAXSETTLEMENTUPDATEService,
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
        
        		this.bo.appendToModelStructure({'global':['FromSendingDate','ToSendingDate','TaxSettlementSendingDetail'],'TaxSettlementSendingDetail':['SendingDate','BalanceYear','Quarter','SendingStatus','TelProtocol','DocProtocol','l_TTaxSettlementSendingDetail_P02','l_TTaxSettlementSendingDetail_P03','l_TTaxSettlementSendingDetail_P04']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_TAXSETTLEMENTUPDATEFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_TAXSETTLEMENTUPDATEComponent, resolver);
    }
} 