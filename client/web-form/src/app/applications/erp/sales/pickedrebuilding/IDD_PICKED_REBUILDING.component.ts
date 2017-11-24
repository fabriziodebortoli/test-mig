import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_PICKED_REBUILDINGService } from './IDD_PICKED_REBUILDING.service';

@Component({
    selector: 'tb-IDD_PICKED_REBUILDING',
    templateUrl: './IDD_PICKED_REBUILDING.component.html',
    providers: [IDD_PICKED_REBUILDINGService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_PICKED_REBUILDINGComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_PICKED_REBUILDINGService,
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
        
        		this.bo.appendToModelStructure({'global':['HFItems_All','HFItems_Range','HFItems_From','HFItems_To','nCurrentElement','GaugeDescription']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_PICKED_REBUILDINGFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_PICKED_REBUILDINGComponent, resolver);
    }
} 