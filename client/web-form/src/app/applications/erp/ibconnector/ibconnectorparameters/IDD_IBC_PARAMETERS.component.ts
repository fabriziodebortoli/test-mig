import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_IBC_PARAMETERSService } from './IDD_IBC_PARAMETERS.service';

@Component({
    selector: 'tb-IDD_IBC_PARAMETERS',
    templateUrl: './IDD_IBC_PARAMETERS.component.html',
    providers: [IDD_IBC_PARAMETERSService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_IBC_PARAMETERSComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_IBC_DOCTYPE_itemSource: any;

    constructor(document: IDD_IBC_PARAMETERSService,
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
        this.IDC_IBC_DOCTYPE_itemSource = {
  "name": "DocumentTypeEnumCombo",
  "namespace": "ERP.IBConnector.Services.IBCDocTypeItemSource"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'IBCConfigurations':['Configuration','DateType','StartDate','EndDate','Days'],'global':['IBCDocuments','IBCCustCollected','IBCCustExpired','IBCCustCollected','IBCSuppCollected','IBCCustCollected','IBCCustCollected','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'IBCDocuments':['DocumentType','DocumentClass','DocumentCycle','ValueSign','CostSign','CommissionSign','QuantitySign'],'IBCCustCollected':['AgingPeriod','AgingPeriodDescription','AgingPeriod','AgingPeriodDescription','AgingPeriod','AgingPeriodDescription','AgingPeriod','AgingPeriodDescription'],'IBCCustExpired':['AgingPeriod','AgingPeriodDescription'],'IBCSuppCollected':['AgingPeriod','AgingPeriodDescription']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_IBC_PARAMETERSFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_IBC_PARAMETERSComponent, resolver);
    }
} 