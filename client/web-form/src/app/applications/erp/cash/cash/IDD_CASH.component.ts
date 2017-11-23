import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_CASHService } from './IDD_CASH.service';

@Component({
    selector: 'tb-IDD_CASH',
    templateUrl: './IDD_CASH.component.html',
    providers: [IDD_CASHService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_CASHComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_CASHService,
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
        
        		this.bo.appendToModelStructure({'Cash':['Cash','Description','WorkerID','CashStubBook','PreferredCurrency','AlternativeCurrency'],'HKLWorkers':['NameComplete'],'HKLStubBook':['Description'],'global':['CashAccounts','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'CashAccounts':['Currency','Account'],'HKLAccount':['Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_CASHFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_CASHComponent, resolver);
    }
} 