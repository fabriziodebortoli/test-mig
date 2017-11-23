import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_CASH_STUBBOOK_NUMERATORService } from './IDD_CASH_STUBBOOK_NUMERATOR.service';

@Component({
    selector: 'tb-IDD_CASH_STUBBOOK_NUMERATOR',
    templateUrl: './IDD_CASH_STUBBOOK_NUMERATOR.component.html',
    providers: [IDD_CASH_STUBBOOK_NUMERATORService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_CASH_STUBBOOK_NUMERATORComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_CASH_STUBBOOK_NUMERATORService,
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
		boService.appendToModelStructure({'StubBookNumbers':['BalanceYear','CashStubBook','Suffix','LastDocDate','LastDocNo'],'HKLStubBook':['Description'],'global':['ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_CASH_STUBBOOK_NUMERATORFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_CASH_STUBBOOK_NUMERATORComponent, resolver);
    }
} 