import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_ADDITIONAL_CHARGES_PARAMETERSService } from './IDD_ADDITIONAL_CHARGES_PARAMETERS.service';

@Component({
    selector: 'tb-IDD_ADDITIONAL_CHARGES_PARAMETERS',
    templateUrl: './IDD_ADDITIONAL_CHARGES_PARAMETERS.component.html',
    providers: [IDD_ADDITIONAL_CHARGES_PARAMETERSService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_ADDITIONAL_CHARGES_PARAMETERSComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_AC_PAR_SPREADINGTEMPLATE_itemSource: any;

    constructor(document: IDD_ADDITIONAL_CHARGES_PARAMETERSService,
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
        this.IDC_AC_PAR_SPREADINGTEMPLATE_itemSource = {
  "name": "DistributionTemplateCombo",
  "namespace": "ERP.AdditionalCharges.AddOnsPurchases.DistributionTemplateCombo"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['AddChgParamSpreadingTemplate']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_ADDITIONAL_CHARGES_PARAMETERSFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_ADDITIONAL_CHARGES_PARAMETERSComponent, resolver);
    }
} 