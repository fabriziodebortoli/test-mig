import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_COMMISSIONS_REBUILDINGService } from './IDD_COMMISSIONS_REBUILDING.service';

@Component({
    selector: 'tb-IDD_COMMISSIONS_REBUILDING',
    templateUrl: './IDD_COMMISSIONS_REBUILDING.component.html',
    providers: [IDD_COMMISSIONS_REBUILDINGService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_COMMISSIONS_REBUILDINGComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_COMMISSIONS_REBUILDINGService,
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
        
        		this.bo.appendToModelStructure({'global':['StartingDate','EndingDate','bAllSalesPeople','bSalesPeopleSel','FromSalesperson','ToSalesperson','bUpdatePolicies','bUpdateSalesPeopleData','nCurrentElement','GaugeDescription']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_COMMISSIONS_REBUILDINGFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_COMMISSIONS_REBUILDINGComponent, resolver);
    }
} 