import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_TD_MRP_SCHEDService } from './IDD_TD_MRP_SCHED.service';

@Component({
    selector: 'tb-IDD_TD_MRP_SCHED',
    templateUrl: './IDD_TD_MRP_SCHED.component.html',
    providers: [IDD_TD_MRP_SCHEDService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_TD_MRP_SCHEDComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_TD_MRP_SCHEDService,
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
        
        		this.bo.appendToModelStructure({'global':['Simulation','Description','RunDate','Status','NumMO','NumRDA']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_TD_MRP_SCHEDFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_TD_MRP_SCHEDComponent, resolver);
    }
} 