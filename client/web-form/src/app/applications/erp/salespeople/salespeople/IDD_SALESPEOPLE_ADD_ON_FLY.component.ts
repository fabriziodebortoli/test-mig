import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_SALESPEOPLE_ADD_ON_FLYService } from './IDD_SALESPEOPLE_ADD_ON_FLY.service';

@Component({
    selector: 'tb-IDD_SALESPEOPLE_ADD_ON_FLY',
    templateUrl: './IDD_SALESPEOPLE_ADD_ON_FLY.component.html',
    providers: [IDD_SALESPEOPLE_ADD_ON_FLYService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_SALESPEOPLE_ADD_ON_FLYComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_SALESPEOPLEADDONFLY_HEAD_AREA_PRIMARY_itemSource: any;

    constructor(document: IDD_SALESPEOPLE_ADD_ON_FLYService,
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
        this.IDC_SALESPEOPLEADDONFLY_HEAD_AREA_PRIMARY_itemSource = {
  "name": "AreaManagerSalesPeopleCombo",
  "namespace": "ERP.SalesPeople.Documents.AreaCombo"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'SalesPeople':['Salesperson','Supplier','Name','IsAnEmployee','Disabled','Policy','IsAnAreaManager','AreaManager','Enasarco','HiringDate','IsACompany','IsACorporation','OneFirmOnly'],'HKLAreaManager':['Name'],'global':['ENASARCONo','bSalespersonMulti','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_SALESPEOPLE_ADD_ON_FLYFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_SALESPEOPLE_ADD_ON_FLYComponent, resolver);
    }
} 