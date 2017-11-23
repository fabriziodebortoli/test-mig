import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_ITEMS_WAP_TRANS_REBUILDService } from './IDD_ITEMS_WAP_TRANS_REBUILD.service';

@Component({
    selector: 'tb-IDD_ITEMS_WAP_TRANS_REBUILD',
    templateUrl: './IDD_ITEMS_WAP_TRANS_REBUILD.component.html',
    providers: [IDD_ITEMS_WAP_TRANS_REBUILDService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_ITEMS_WAP_TRANS_REBUILDComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_ITEMS_WAP_TRANS_REBUILDService,
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
		boService.appendToModelStructure({'global':['HFPeriod_From','HFPeriod_To','HFItems_All','HFItems_Range','HFItems_From','HFItems_To','nCurrentElement','GaugeDescription']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_ITEMS_WAP_TRANS_REBUILDFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_ITEMS_WAP_TRANS_REBUILDComponent, resolver);
    }
} 