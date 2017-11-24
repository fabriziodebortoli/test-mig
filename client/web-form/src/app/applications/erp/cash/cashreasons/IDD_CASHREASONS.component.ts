import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_CASHREASONSService } from './IDD_CASHREASONS.service';

@Component({
    selector: 'tb-IDD_CASHREASONS',
    templateUrl: './IDD_CASHREASONS.component.html',
    providers: [IDD_CASHREASONSService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_CASHREASONSComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_CASHREASONSService,
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
        
        		this.bo.appendToModelStructure({'CashReasons':['Reason','Description','OperationType','DocNoIsMand','AutoNumbering','AutoPrint','Account','AccTpl','AccRsn','CostCenter','Job'],'HKLAccount':['Description'],'HKLAccTpl':['Description'],'HKLAccRsn':['Description'],'HKLCostCenters':['Description'],'HKLJobs':['Description'],'global':['ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_CASHREASONSFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_CASHREASONSComponent, resolver);
    }
} 