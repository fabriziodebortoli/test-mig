import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_PARAMETERS_PAYABLESRECEIVABLESService } from './IDD_PARAMETERS_PAYABLESRECEIVABLES.service';

@Component({
    selector: 'tb-IDD_PARAMETERS_PAYABLESRECEIVABLES',
    templateUrl: './IDD_PARAMETERS_PAYABLESRECEIVABLES.component.html',
    providers: [IDD_PARAMETERS_PAYABLESRECEIVABLESService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_PARAMETERS_PAYABLESRECEIVABLESComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_APAR_PARAMETERS_PRINTS_TYPE_itemSource: any;

    constructor(document: IDD_PARAMETERS_PAYABLESRECEIVABLESService,
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
        this.IDC_APAR_PARAMETERS_PRINTS_TYPE_itemSource = {
  "name": "FiscalPrintoutsEnumCombo",
  "namespace": "ERP.Accounting.Components.FiscalPrintoutsEnumCombo"
}; 

        		this.bo.appendToModelStructure({'global':['PymtTerms','RequestsForPaymt','ParametersRate','CustomizedPrint','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'PymtTerms':['PaymentTerm','CollectionAccTpl','CollectionAccRsn','PaymentAccTpl','PaymentAccRsn'],'RequestsForPaymt':['Line','DescriptiveText'],'ParametersRate':['Line','FromDate','ToDate','InterestRate','Disabled'],'CustomizedPrint':['CodeType','DescriptiveText']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_PARAMETERS_PAYABLESRECEIVABLESFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_PARAMETERS_PAYABLESRECEIVABLESComponent, resolver);
    }
} 