import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_NUMERATOR_BILLSService } from './IDD_NUMERATOR_BILLS.service';

@Component({
    selector: 'tb-IDD_NUMERATOR_BILLS',
    templateUrl: './IDD_NUMERATOR_BILLS.component.html',
    providers: [IDD_NUMERATOR_BILLSService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_NUMERATOR_BILLSComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_NUMERATOR_BILLSService,
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
		boService.appendToModelStructure({'StubBookNumbers':['BalanceYear','StubBook','Suffix','LastDocDate','LastDocNo'],'HKLStubBook':['Description'],'global':['ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_NUMERATOR_BILLSFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_NUMERATOR_BILLSComponent, resolver);
    }
} 