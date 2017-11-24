import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_WAP_VALUATION_DATA_UPDATEService } from './IDD_WAP_VALUATION_DATA_UPDATE.service';

@Component({
    selector: 'tb-IDD_WAP_VALUATION_DATA_UPDATE',
    templateUrl: './IDD_WAP_VALUATION_DATA_UPDATE.component.html',
    providers: [IDD_WAP_VALUATION_DATA_UPDATEService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_WAP_VALUATION_DATA_UPDATEComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_WAP_VALUATION_DATA_UPDATEService,
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
        
        		this.bo.appendToModelStructure({'global':['HFPeriod_From','HFPeriod_To','nCurrentElement','GaugeDescription']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_WAP_VALUATION_DATA_UPDATEFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_WAP_VALUATION_DATA_UPDATEComponent, resolver);
    }
} 