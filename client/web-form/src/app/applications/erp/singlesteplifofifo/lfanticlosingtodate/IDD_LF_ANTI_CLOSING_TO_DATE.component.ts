import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_LF_ANTI_CLOSING_TO_DATEService } from './IDD_LF_ANTI_CLOSING_TO_DATE.service';

@Component({
    selector: 'tb-IDD_LF_ANTI_CLOSING_TO_DATE',
    templateUrl: './IDD_LF_ANTI_CLOSING_TO_DATE.component.html',
    providers: [IDD_LF_ANTI_CLOSING_TO_DATEService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_LF_ANTI_CLOSING_TO_DATEComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_LF_ANTI_CLOSING_TO_DATEService,
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
		boService.appendToModelStructure({'global':['HFItems_All','HFItems_Range','HFItems_From','HFItems_To','StartingDate','OpeningDate','nCurrentElement','GaugeDescription']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_LF_ANTI_CLOSING_TO_DATEFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_LF_ANTI_CLOSING_TO_DATEComponent, resolver);
    }
} 