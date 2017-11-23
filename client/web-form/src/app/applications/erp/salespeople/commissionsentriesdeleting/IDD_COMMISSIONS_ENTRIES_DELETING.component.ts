import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_COMMISSIONS_ENTRIES_DELETINGService } from './IDD_COMMISSIONS_ENTRIES_DELETING.service';

@Component({
    selector: 'tb-IDD_COMMISSIONS_ENTRIES_DELETING',
    templateUrl: './IDD_COMMISSIONS_ENTRIES_DELETING.component.html',
    providers: [IDD_COMMISSIONS_ENTRIES_DELETINGService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_COMMISSIONS_ENTRIES_DELETINGComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_COMMISSIONS_ENTRIES_DELETINGService,
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
        
        		this.bo.appendToModelStructure({'global':['bAllSalesPeople','bSalesPeopleSel','FromSalesperson','ToSalesperson','StartingDate','EndingDate','FromOustandingDate','ToOustandingDate','bDeleteGenerateFlag','bOutstandingProcess','bEntriesProcess','nCurrentElement','GaugeDescription']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_COMMISSIONS_ENTRIES_DELETINGFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_COMMISSIONS_ENTRIES_DELETINGComponent, resolver);
    }
} 