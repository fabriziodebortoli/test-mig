import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_BR_SERVICE_TYPESService } from './IDD_BR_SERVICE_TYPES.service';

@Component({
    selector: 'tb-IDD_BR_SERVICE_TYPES',
    templateUrl: './IDD_BR_SERVICE_TYPES.component.html',
    providers: [IDD_BR_SERVICE_TYPESService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_BR_SERVICE_TYPESComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_BR_SERVICE_TYPES_DETAIL_TAX_RATE_TYPE_itemSource: any;

    constructor(document: IDD_BR_SERVICE_TYPESService,
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
        this.IDC_BR_SERVICE_TYPES_DETAIL_TAX_RATE_TYPE_itemSource = {
  "name": "BRServiceTypesEnumCombo",
  "namespace": "ERP.Business_BR.Components.BRServiceTypesEnumCombo"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'DBTBRServiceTypes':['ServiceTypeCode','Description','Disabled'],'global':['DBTBRServiceTypesDetail','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'DBTBRServiceTypesDetail':['TaxRateType','TaxRateCode'],'HKLBRTaxRate':['Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_BR_SERVICE_TYPESFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_BR_SERVICE_TYPESComponent, resolver);
    }
} 