import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_BIN_MAINTENANCEService } from './IDD_BIN_MAINTENANCE.service';

@Component({
    selector: 'tb-IDD_BIN_MAINTENANCE',
    templateUrl: './IDD_BIN_MAINTENANCE.component.html',
    providers: [IDD_BIN_MAINTENANCEService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_BIN_MAINTENANCEComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_BIN_MAINTENANCEService,
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
        
        		this.bo.appendToModelStructure({'global':['Storage','Zone','bAllBin','bSelBin','FromBin','ToBin','bBlocked','bUnBlocked','bEnable','bDisable','bBinMainCancel','DBTEnhBinMaintenance'],'DBTEnhBinMaintenance':['l_Selection','Bin','Storage','Zone','Section','Disabled','Blocked']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_BIN_MAINTENANCEFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_BIN_MAINTENANCEComponent, resolver);
    }
} 