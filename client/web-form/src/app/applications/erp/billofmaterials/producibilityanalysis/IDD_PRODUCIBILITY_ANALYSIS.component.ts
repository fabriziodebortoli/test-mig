import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_PRODUCIBILITY_ANALYSISService } from './IDD_PRODUCIBILITY_ANALYSIS.service';

@Component({
    selector: 'tb-IDD_PRODUCIBILITY_ANALYSIS',
    templateUrl: './IDD_PRODUCIBILITY_ANALYSIS.component.html',
    providers: [IDD_PRODUCIBILITY_ANALYSISService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_PRODUCIBILITY_ANALYSISComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_PRODUCIBILITY_ANALYSISService,
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
		boService.appendToModelStructure({'global':['Plan','DBTSummaryDetail'],'HKLPlan':['Description'],'DBTSummaryDetail':['l_LineSummaryDescription']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_PRODUCIBILITY_ANALYSISFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_PRODUCIBILITY_ANALYSISComponent, resolver);
    }
} 