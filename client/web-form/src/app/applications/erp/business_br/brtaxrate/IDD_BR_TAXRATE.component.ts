import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_BR_TAXRATEService } from './IDD_BR_TAXRATE.service';

@Component({
    selector: 'tb-IDD_BR_TAXRATE',
    templateUrl: './IDD_BR_TAXRATE.component.html',
    providers: [IDD_BR_TAXRATEService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_BR_TAXRATEComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_BR_TAXRATE_TAX_TYPE_itemSource: any;

    constructor(document: IDD_BR_TAXRATEService,
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
        this.IDC_BR_TAXRATE_TAX_TYPE_itemSource = {
  "name": "BRTaxRateEnumCombo",
  "namespace": "ERP.Business_BR.Components.BRTaxRateEnumCombo"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'DBTBRTaxRate':['TaxRateCode','Description','TaxType','TaxRate','NotTaxable','Thresold','ValidityStartingDate','ValidityEndingDate'],'global':['DBTBRTaxRateDetail','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'DBTBRTaxRateDetail':['MinAmount','MaxAmount','TaxRate']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_BR_TAXRATEFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_BR_TAXRATEComponent, resolver);
    }
} 