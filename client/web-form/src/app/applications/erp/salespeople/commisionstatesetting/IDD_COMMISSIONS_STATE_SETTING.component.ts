import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_COMMISSIONS_STATE_SETTINGService } from './IDD_COMMISSIONS_STATE_SETTING.service';

@Component({
    selector: 'tb-IDD_COMMISSIONS_STATE_SETTING',
    templateUrl: './IDD_COMMISSIONS_STATE_SETTING.component.html',
    providers: [IDD_COMMISSIONS_STATE_SETTINGService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_COMMISSIONS_STATE_SETTINGComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_COMMISSIONS_STATE_SETTINGService,
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
		boService.appendToModelStructure({'global':['bAllPeriod','bPeriodSel','StartingDate','EndingDate','bManualize','bAutomate','bOrdersProcess','bDocProcess','nCurrentElement','GaugeDescription']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_COMMISSIONS_STATE_SETTINGFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_COMMISSIONS_STATE_SETTINGComponent, resolver);
    }
} 