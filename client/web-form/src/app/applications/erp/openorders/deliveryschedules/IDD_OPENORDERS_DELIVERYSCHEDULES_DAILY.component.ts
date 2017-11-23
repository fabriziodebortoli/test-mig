import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_OPENORDERS_DELIVERYSCHEDULES_DAILYService } from './IDD_OPENORDERS_DELIVERYSCHEDULES_DAILY.service';

@Component({
    selector: 'tb-IDD_OPENORDERS_DELIVERYSCHEDULES_DAILY',
    templateUrl: './IDD_OPENORDERS_DELIVERYSCHEDULES_DAILY.component.html',
    providers: [IDD_OPENORDERS_DELIVERYSCHEDULES_DAILYService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_OPENORDERS_DELIVERYSCHEDULES_DAILYComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_OPENORDERS_DELIVERYSCHEDULES_DAILYService,
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
		boService.appendToModelStructure({'global':['DailyDelivery','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'DailyDelivery':['DeliveryDate','DayDate','DailyQty','Notes']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_OPENORDERS_DELIVERYSCHEDULES_DAILYFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_OPENORDERS_DELIVERYSCHEDULES_DAILYComponent, resolver);
    }
} 