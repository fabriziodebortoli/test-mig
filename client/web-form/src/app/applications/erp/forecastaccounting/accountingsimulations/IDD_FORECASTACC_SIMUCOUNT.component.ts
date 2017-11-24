import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_FORECASTACC_SIMUCOUNTService } from './IDD_FORECASTACC_SIMUCOUNT.service';

@Component({
    selector: 'tb-IDD_FORECASTACC_SIMUCOUNT',
    templateUrl: './IDD_FORECASTACC_SIMUCOUNT.component.html',
    providers: [IDD_FORECASTACC_SIMUCOUNTService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_FORECASTACC_SIMUCOUNTComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_FORECASTACC_SIMUCOUNTService,
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
        
        		this.bo.appendToModelStructure({'AccountingSimulations':['Simulation','Description','PostingDate','ValidityEndingDate','OperatorID','Operator','l_OperatorDesc','Signature','Notes'],'global':['ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_FORECASTACC_SIMUCOUNTFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_FORECASTACC_SIMUCOUNTComponent, resolver);
    }
} 