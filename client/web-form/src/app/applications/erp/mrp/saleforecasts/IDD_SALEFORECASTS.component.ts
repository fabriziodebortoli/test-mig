import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_SALEFORECASTSService } from './IDD_SALEFORECASTS.service';

@Component({
    selector: 'tb-IDD_SALEFORECASTS',
    templateUrl: './IDD_SALEFORECASTS.component.html',
    providers: [IDD_SALEFORECASTSService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_SALEFORECASTSComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_SALEFORECASTSService,
        eventData: EventDataService,
        resolver: ComponentFactoryResolver,
        private store: Store,
        ciService: ComponentInfoService,
        changeDetectorRef: ChangeDetectorRef) {
		super(document, eventData, ciService, changeDetectorRef, resolver);
        this.eventData.change.subscribe(() => this.changeDetectorRef.detectChanges());
    }

    ngOnInit() {
        super.ngOnInit();
        
        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['DBTSaleForecasts','DBTWeekly','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'DBTSaleForecasts':['Year','Month','Quantity','SaleQuantity','Job','CostCenter'],'DBTWeekly':['Week','WeekDescr','Quantity']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_SALEFORECASTSFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_SALEFORECASTSComponent, resolver);
    }
} 