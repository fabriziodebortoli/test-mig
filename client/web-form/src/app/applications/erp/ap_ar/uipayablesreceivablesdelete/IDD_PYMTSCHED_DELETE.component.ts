import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_PYMTSCHED_DELETEService } from './IDD_PYMTSCHED_DELETE.service';

@Component({
    selector: 'tb-IDD_PYMTSCHED_DELETE',
    templateUrl: './IDD_PYMTSCHED_DELETE.component.html',
    providers: [IDD_PYMTSCHED_DELETEService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_PYMTSCHED_DELETEComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_PYMTSCHED_DELETEService,
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
		boService.appendToModelStructure({'global':['bAll','bCustSuppSel','CustSuppSel','Description','LimitDate','bTaxExigibilityDelete','bExigibDelete','nCurrentElement','GaugeDescription']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_PYMTSCHED_DELETEFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_PYMTSCHED_DELETEComponent, resolver);
    }
} 