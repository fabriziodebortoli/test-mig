import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_FDLService } from './IDD_FDL.service';

@Component({
    selector: 'tb-IDD_FDL',
    templateUrl: './IDD_FDL.component.html',
    providers: [IDD_FDLService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_FDLComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_FDL_PROVIDER_itemSource: any;

    constructor(document: IDD_FDLService,
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
        this.IDC_FDL_PROVIDER_itemSource = {
  "name": "FixingProviderCombo",
  "namespace": "ERP.Currencies.BatchDocuments.FixingProviderCombo"
}; 

        		this.bo.appendToModelStructure({'global':['strComboProvider','RequestedDate','DownloadedFixingDate','BaseCurrency','BaseCurrencyDescri','FixingDownload'],'FixingDownload':['l_Selected','ReferredCurrency','l_ReferredCurrencyDescription','FixingDate','FixingDate','l_InternationalCode','Fixing','l_NewFixing']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_FDLFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_FDLComponent, resolver);
    }
} 