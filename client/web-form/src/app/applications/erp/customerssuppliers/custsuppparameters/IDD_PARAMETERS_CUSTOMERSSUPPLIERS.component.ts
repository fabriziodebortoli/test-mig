import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_PARAMETERS_CUSTOMERSSUPPLIERSService } from './IDD_PARAMETERS_CUSTOMERSSUPPLIERS.service';

@Component({
    selector: 'tb-IDD_PARAMETERS_CUSTOMERSSUPPLIERS',
    templateUrl: './IDD_PARAMETERS_CUSTOMERSSUPPLIERS.component.html',
    providers: [IDD_PARAMETERS_CUSTOMERSSUPPLIERSService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_PARAMETERS_CUSTOMERSSUPPLIERSComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_PARAM_CUSTSUPP_PRINTS_TYPE_itemSource: any;

    constructor(document: IDD_PARAMETERS_CUSTOMERSSUPPLIERSService,
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
        this.IDC_PARAM_CUSTSUPP_PRINTS_TYPE_itemSource = {
  "name": "FiscalPrintoutsEnumCombo",
  "namespace": "ERP.Accounting.Components.FiscalPrintoutsEnumCombo"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['CustomizedPrint','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'CustomizedPrint':['CodeType','DescriptiveText']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_PARAMETERS_CUSTOMERSSUPPLIERSFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_PARAMETERS_CUSTOMERSSUPPLIERSComponent, resolver);
    }
} 