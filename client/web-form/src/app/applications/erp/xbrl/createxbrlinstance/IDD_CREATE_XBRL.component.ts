import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_CREATE_XBRLService } from './IDD_CREATE_XBRL.service';

@Component({
    selector: 'tb-IDD_CREATE_XBRL',
    templateUrl: './IDD_CREATE_XBRL.component.html',
    providers: [IDD_CREATE_XBRLService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_CREATE_XBRLComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_CREATE_XBRLService,
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
		boService.appendToModelStructure({'global':['Branch','Liquidation','SoleShareholder','UnderCoord','Coordinator','BelongToGroup','GroupLeader','GroupCountry','SlaveDataXBRL','SlaveDataXBRLErrors','Year','Month','DateStart','DateEnd','DateStartPrev','DateEndPrev','FileNameComplete'],'SlaveDataXBRL':['l_TEnhPersonalDataXBRLCrea_P9','l_TEnhPersonalDataXBRLCrea_P1','l_TEnhPersonalDataXBRLCrea_P2','l_TEnhPersonalDataXBRLCrea_P8'],'SlaveDataXBRLErrors':['l_TEnhPersonalDataXBRLCErr_P1']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_CREATE_XBRLFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_CREATE_XBRLComponent, resolver);
    }
} 