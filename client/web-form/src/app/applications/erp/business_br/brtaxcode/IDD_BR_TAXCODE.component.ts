import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_BR_TAXCODEService } from './IDD_BR_TAXCODE.service';

@Component({
    selector: 'tb-IDD_BR_TAXCODE',
    templateUrl: './IDD_BR_TAXCODE.component.html',
    providers: [IDD_BR_TAXCODEService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_BR_TAXCODEComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_BR_TAXCODE_BR_TAXTYPE_itemSource: any;

    constructor(document: IDD_BR_TAXCODEService,
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
        this.IDC_BR_TAXCODE_BR_TAXTYPE_itemSource = {
  "name": "BRTaxTypeEnumCombo",
  "namespace": "ERP.Business_BR.Components.BRTaxTypeEnumCombo"
}; 

        		this.bo.appendToModelStructure({'DBTBRTaxCode':['TaxCode','Description','TaxType','Receipt','Issue','ValidityStartingDate','ValidityEndingDate'],'global':['ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_BR_TAXCODEFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_BR_TAXCODEComponent, resolver);
    }
} 