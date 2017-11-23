import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_TD_BR_SPED_GENERATIONService } from './IDD_TD_BR_SPED_GENERATION.service';

@Component({
    selector: 'tb-IDD_TD_BR_SPED_GENERATION',
    templateUrl: './IDD_TD_BR_SPED_GENERATION.component.html',
    providers: [IDD_TD_BR_SPED_GENERATIONService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_TD_BR_SPED_GENERATIONComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_BR_SPED_GENERATION_FILE_TYPE_itemSource: any;

    constructor(document: IDD_TD_BR_SPED_GENERATIONService,
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
        this.IDC_BR_SPED_GENERATION_FILE_TYPE_itemSource = {
  "name": "SPEDFileTypeComboBox",
  "namespace": "ERP.Business_BR.Documents.SPEDFileTypeComboBox"
}; 

        		this.bo.appendToModelStructure({'global':['StartDate','EndDate','FileType','ICMSPaymentDate','ICMSSTPaymentDate','bIncludeInvData','InvDate','DBTSummaryDetail'],'DBTSummaryDetail':['l_LineSummaryDescription']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_TD_BR_SPED_GENERATIONFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_TD_BR_SPED_GENERATIONComponent, resolver);
    }
} 