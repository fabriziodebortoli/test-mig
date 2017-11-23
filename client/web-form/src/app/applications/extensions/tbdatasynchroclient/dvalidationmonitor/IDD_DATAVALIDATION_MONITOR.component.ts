import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_DATAVALIDATION_MONITORService } from './IDD_DATAVALIDATION_MONITOR.service';

@Component({
    selector: 'tb-IDD_DATAVALIDATION_MONITOR',
    templateUrl: './IDD_DATAVALIDATION_MONITOR.component.html',
    providers: [IDD_DATAVALIDATION_MONITORService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_DATAVALIDATION_MONITORComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_DATAVALIDATION_MONITORService,
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
		boService.appendToModelStructure({'global':['ProviderName','DocNamespace','bAllDate','bSelectionDate','DateFrom','DateTo','bAutoRefresh','PictureStatus','nValueGauge','GaugeDescription','ValidationMonitorDocSummary','ValidationInfoMonitor'],'ValidationMonitorDocSummary':['DocNamespace','NoErrors'],'ValidationInfoMonitor':['TEnhDS_Valid_Code','TEnhDS_Valid_Description','DocNamespace','TEnhDS_Valid_FormattedMsgError','ValidationDate','DocTBGuid','ActionName','FKError','XSDError']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_DATAVALIDATION_MONITORFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_DATAVALIDATION_MONITORComponent, resolver);
    }
} 