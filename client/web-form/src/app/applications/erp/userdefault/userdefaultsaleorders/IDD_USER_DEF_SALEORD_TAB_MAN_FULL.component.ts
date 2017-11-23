import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_USER_DEF_SALEORD_TAB_MAN_FULLService } from './IDD_USER_DEF_SALEORD_TAB_MAN_FULL.service';

@Component({
    selector: 'tb-IDD_USER_DEF_SALEORD_TAB_MAN_FULL',
    templateUrl: './IDD_USER_DEF_SALEORD_TAB_MAN_FULL.component.html',
    providers: [IDD_USER_DEF_SALEORD_TAB_MAN_FULLService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_USER_DEF_SALEORD_TAB_MAN_FULLComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_USER_DEF_SALEORD_TAB_MAN_FULLService,
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
        
        		this.bo.appendToModelStructure({'global':['bAllBranches','bAllWorkers','UserDefaultSaleOrdersByDocType','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'UserDefaultSaleOrders':['Branch','BranchDesc','WorkerID','WorkerDesc']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_USER_DEF_SALEORD_TAB_MAN_FULLFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_USER_DEF_SALEORD_TAB_MAN_FULLComponent, resolver);
    }
} 