import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_ITEMBALService } from './IDD_ITEMBAL.service';

@Component({
    selector: 'tb-IDD_ITEMBAL',
    templateUrl: './IDD_ITEMBAL.component.html',
    providers: [IDD_ITEMBALService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_ITEMBALComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_ITEMBALService,
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
        
        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['bAllFiscalYear','bSelFiscalYear','FromFiscalYear','ToFiscalYear','bAllItems','bItemSel','FromItem','ToItem','bIgnoreNotTransactableItems','bIgnoreDisabledItems','bUseLotsBalances','bUseVariantsBalances','bMigrateAlsoStorageQtyLower','bMigrateAlsoStorageQtyGreater','DBTItemBalMigrStorages','nCurrentElement','GaugeDescription'],'DBTItemBalMigrStorages':['IsMainStorage','Storage','Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_ITEMBALFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_ITEMBALComponent, resolver);
    }
} 