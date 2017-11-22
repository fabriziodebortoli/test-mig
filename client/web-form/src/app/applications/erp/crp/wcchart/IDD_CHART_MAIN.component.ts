import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_CHART_MAINService } from './IDD_CHART_MAIN.service';

@Component({
    selector: 'tb-IDD_CHART_MAIN',
    templateUrl: './IDD_CHART_MAIN.component.html',
    providers: [IDD_CHART_MAINService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_CHART_MAINComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_CHART_MAINService,
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
		boService.appendToModelStructure({'global':['dDateFrom','dDateTo','bAllWC','bWCSel','sFromWC','sToWC','eWCType','WCChart'],'WCChart':['Selected','WC','CodeType','Outsourced','Supplier','CompanyName'],'HKLWC':['Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_CHART_MAINFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_CHART_MAINComponent, resolver);
    }
} 