import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_DECLARATION_OF_INTENT_DELETEService } from './IDD_DECLARATION_OF_INTENT_DELETE.service';

@Component({
    selector: 'tb-IDD_DECLARATION_OF_INTENT_DELETE',
    templateUrl: './IDD_DECLARATION_OF_INTENT_DELETE.component.html',
    providers: [IDD_DECLARATION_OF_INTENT_DELETEService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_DECLARATION_OF_INTENT_DELETEComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_DECLARATION_OF_INTENT_DELETEService,
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
		boService.appendToModelStructure({'global':['bAll','bCustSuppSel','CustSuppSel','Description','FromDate','LimitDate','FromDocNo','ToDocNo','DBTSummaryDetail'],'DBTSummaryDetail':['l_LineSummaryDescription']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_DECLARATION_OF_INTENT_DELETEFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_DECLARATION_OF_INTENT_DELETEComponent, resolver);
    }
} 