import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_PARAMETERSService } from './IDD_PARAMETERS.service';

@Component({
    selector: 'tb-IDD_PARAMETERS',
    templateUrl: './IDD_PARAMETERS.component.html',
    providers: [IDD_PARAMETERSService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_PARAMETERSComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_PARAMETERS_PRINTS_TYPE_itemSource: any;

    constructor(document: IDD_PARAMETERSService,
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
        this.IDC_PARAMETERS_PRINTS_TYPE_itemSource = {
  "name": "FiscalPrintoutsEnumCombo",
  "namespace": "ERP.Accounting.Components.FiscalPrintoutsEnumCombo"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['ParametersCoeff','CustomizedPrint','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'ParametersCoeff':['FromPeriod','ToPeriod','RegrCoeff'],'CustomizedPrint':['CodeType','DescriptiveText']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_PARAMETERSFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_PARAMETERSComponent, resolver);
    }
} 