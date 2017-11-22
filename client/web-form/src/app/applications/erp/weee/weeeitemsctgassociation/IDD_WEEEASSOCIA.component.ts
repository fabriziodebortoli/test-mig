import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_WEEEASSOCIAService } from './IDD_WEEEASSOCIA.service';

@Component({
    selector: 'tb-IDD_WEEEASSOCIA',
    templateUrl: './IDD_WEEEASSOCIA.component.html',
    providers: [IDD_WEEEASSOCIAService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_WEEEASSOCIAComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_WEEEASSOCIAService,
        eventData: EventDataService,
        resolver: ComponentFactoryResolver,
        private store: Store,
        ciService: ComponentInfoService,
        private changeDetectorRef: ChangeDetectorRef) {
        super(document, eventData, resolver, ciService);
        this.eventData.change.subscribe(() => this.changeDetectorRef.detectChanges());
    }

    ngOnInit() {
        super.ngOnInit();
        
        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['HFItems_All','HFItems_Range','HFItems_From','HFItems_To','Equal','WEEECtg','WEEECtg2','nCurrentElement','GaugeDescription'],'HKLWEEECtg':['Description'],'HKLWEEECtg2':['Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_WEEEASSOCIAFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_WEEEASSOCIAComponent, resolver);
    }
} 