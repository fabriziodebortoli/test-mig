import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_RECLASS_UPGRADEService } from './IDD_RECLASS_UPGRADE.service';

@Component({
    selector: 'tb-IDD_RECLASS_UPGRADE',
    templateUrl: './IDD_RECLASS_UPGRADE.component.html',
    providers: [IDD_RECLASS_UPGRADEService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_RECLASS_UPGRADEComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_RECLASS_UPGRADEService,
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
		boService.appendToModelStructure({'global':['Normal','Abbreviated','BASEL','Assets','New_Assets','Liabilities','New_Liabilities','ProfitLoss','New_ProfitLoss','ProfitLossPert','New_ProfitLossPert','SlaveOldDataReclassUpg','SlaveNewDataReclassUpg'],'SlaveOldDataReclassUpg':['l_TEnhOldDataReclassUpg_P1','l_Old_Code','l_TEnhOldDataReclassUpg_P2','l_TEnhOldDataReclassUpg_P3','l_TEnhOldDataReclassUpg_P4','l_TEnhOldDataReclassUpg_P5'],'SlaveNewDataReclassUpg':['l_TEnhNewDataReclassUpg_P1','l_TEnhNewDataReclassUpg_P2','l_TEnhNewDataReclassUpg_P3','l_TEnhNewDataReclassUpg_P3','l_TEnhNewDataReclassUpg_P8','l_TEnhNewDataReclassUpg_P4','l_TEnhNewDataReclassUpg_P5','l_TEnhNewDataReclassUpg_P6','l_TEnhNewDataReclassUpg_P7']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_RECLASS_UPGRADEFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_RECLASS_UPGRADEComponent, resolver);
    }
} 