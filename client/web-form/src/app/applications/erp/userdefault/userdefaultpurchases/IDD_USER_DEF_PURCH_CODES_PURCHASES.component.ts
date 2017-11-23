import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_USER_DEF_PURCH_CODES_PURCHASESService } from './IDD_USER_DEF_PURCH_CODES_PURCHASES.service';

@Component({
    selector: 'tb-IDD_USER_DEF_PURCH_CODES_PURCHASES',
    templateUrl: './IDD_USER_DEF_PURCH_CODES_PURCHASES.component.html',
    providers: [IDD_USER_DEF_PURCH_CODES_PURCHASESService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_USER_DEF_PURCH_CODES_PURCHASESComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_USER_DEF_PURCH_CODES_PURCHASESService,
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
		boService.appendToModelStructure({'global':['bAllBranches','bAllWorkers','UserDefaultPurchasesByDocType','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'UserDefaultPurchases':['Branch','BranchDesc','WorkerID','WorkerDesc']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_USER_DEF_PURCH_CODES_PURCHASESFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_USER_DEF_PURCH_CODES_PURCHASESComponent, resolver);
    }
} 