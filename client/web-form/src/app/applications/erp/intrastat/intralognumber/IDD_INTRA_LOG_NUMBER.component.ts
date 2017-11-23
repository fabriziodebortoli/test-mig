import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_INTRA_LOG_NUMBERService } from './IDD_INTRA_LOG_NUMBER.service';

@Component({
    selector: 'tb-IDD_INTRA_LOG_NUMBER',
    templateUrl: './IDD_INTRA_LOG_NUMBER.component.html',
    providers: [IDD_INTRA_LOG_NUMBERService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_INTRA_LOG_NUMBERComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_INTRA_LOG_NUMBERService,
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
		boService.appendToModelStructure({'global':['IntraLogNumberDetails','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'IntraLogNumberDetails':['BalanceYear','BalanceMonth','Dispatches','LogNo','SendingDate','TEnhIntraLogNumberDetail_P1']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_INTRA_LOG_NUMBERFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_INTRA_LOG_NUMBERComponent, resolver);
    }
} 