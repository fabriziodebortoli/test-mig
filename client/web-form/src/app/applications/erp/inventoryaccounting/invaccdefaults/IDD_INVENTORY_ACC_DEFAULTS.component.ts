import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_INVENTORY_ACC_DEFAULTSService } from './IDD_INVENTORY_ACC_DEFAULTS.service';

@Component({
    selector: 'tb-IDD_INVENTORY_ACC_DEFAULTS',
    templateUrl: './IDD_INVENTORY_ACC_DEFAULTS.component.html',
    providers: [IDD_INVENTORY_ACC_DEFAULTSService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_INVENTORY_ACC_DEFAULTSComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_INVACCDEFAULTS_BE_SYMBOL_itemSource: any;

    constructor(document: IDD_INVENTORY_ACC_DEFAULTSService,
        eventData: EventDataService,
        resolver: ComponentFactoryResolver,
        private store: Store,
        ciService: ComponentInfoService,
        private changeDetectorRef: ChangeDetectorRef) {
        super(document, eventData, resolver, ciService);
        this.eventData.change.subscribe(() => this.changeDetectorRef.detectChanges());
    }

    ngOnInit() {
        super.ngOnInit();
        this.IDC_INVACCDEFAULTS_BE_SYMBOL_itemSource = {
  "name": "ItemSourceXml",
  "namespace": "ItemSource.Framework.TbGes.TbGes.ItemSourceXml",
  "parameter": "DataFile.ERP.InventoryAccounting.OffsetSymbols",
  "useProductLanguage": true
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['Detail','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'Detail':['OffsetSymbol','OffsetSymbolDescription','Offset'],'HKLOffsetDetail':['Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_INVENTORY_ACC_DEFAULTSFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_INVENTORY_ACC_DEFAULTSComponent, resolver);
    }
} 