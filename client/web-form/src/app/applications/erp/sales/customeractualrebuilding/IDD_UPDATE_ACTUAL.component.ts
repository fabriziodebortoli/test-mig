import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_UPDATE_ACTUALService } from './IDD_UPDATE_ACTUAL.service';

@Component({
    selector: 'tb-IDD_UPDATE_ACTUAL',
    templateUrl: './IDD_UPDATE_ACTUAL.component.html',
    providers: [IDD_UPDATE_ACTUALService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_UPDATE_ACTUALComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_UPDATE_ACTUALService,
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
        
        		this.bo.appendToModelStructure({'global':['SelectorStartingDate','SelectorEndingDate','SelectorAllCustomer','SelectorCustomerSel','SelectorFromCustomer','SelectorToCustomer','ActualClear','nCurrentElement','GaugeDescription']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_UPDATE_ACTUALFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_UPDATE_ACTUALComponent, resolver);
    }
} 