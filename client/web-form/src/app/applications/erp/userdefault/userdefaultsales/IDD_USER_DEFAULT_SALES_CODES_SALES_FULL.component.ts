import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_USER_DEFAULT_SALES_CODES_SALES_FULLService } from './IDD_USER_DEFAULT_SALES_CODES_SALES_FULL.service';

@Component({
    selector: 'tb-IDD_USER_DEFAULT_SALES_CODES_SALES_FULL',
    templateUrl: './IDD_USER_DEFAULT_SALES_CODES_SALES_FULL.component.html',
    providers: [IDD_USER_DEFAULT_SALES_CODES_SALES_FULLService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_USER_DEFAULT_SALES_CODES_SALES_FULLComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_USER_DEFAULT_SALES_CODES_SALES_FULLService,
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
		boService.appendToModelStructure({'global':['bAllBranches','bAllWorkers','UserDefaultSalesByDocType','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'UserDefaultSales':['Branch','BranchDesc','WorkerID','WorkerDesc']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_USER_DEFAULT_SALES_CODES_SALES_FULLFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_USER_DEFAULT_SALES_CODES_SALES_FULLComponent, resolver);
    }
} 