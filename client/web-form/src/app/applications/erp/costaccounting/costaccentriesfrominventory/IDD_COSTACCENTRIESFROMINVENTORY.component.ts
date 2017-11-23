import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_COSTACCENTRIESFROMINVENTORYService } from './IDD_COSTACCENTRIESFROMINVENTORY.service';

@Component({
    selector: 'tb-IDD_COSTACCENTRIESFROMINVENTORY',
    templateUrl: './IDD_COSTACCENTRIESFROMINVENTORY.component.html',
    providers: [IDD_COSTACCENTRIESFROMINVENTORYService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_COSTACCENTRIESFROMINVENTORYComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_BATCH_CA_MM_REASON_itemSource: any;
public IDC_BATCH_CA_MM_VALUE_itemSource: any;

    constructor(document: IDD_COSTACCENTRIESFROMINVENTORYService,
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
        this.IDC_BATCH_CA_MM_REASON_itemSource = {
  "name": "ReasonsEnumCombo",
  "namespace": "ERP.CostAccounting.Documents.ReasonsEnumCombo"
}; 
this.IDC_BATCH_CA_MM_VALUE_itemSource = {
  "name": "InvEntryValueCombo",
  "namespace": "ERP.Company.Components.ValuationInventoryCombo"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['InvEntryFromPostDate','InvEntryToPostDate','InvEntryProcessed','InvEntryRsnAll','InvEntryRsnSel','InvEntryRsnType','ReasonFrom','ReasonTo','InvEntryInvValue','InvEntryInvLineCost','InvEntryValueCalc','InvEntryValue','InvEntryInputDate','InvEntryCostAccPostDate','InvEntryDocPostDate','nCurrentElement','GaugeDescription']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_COSTACCENTRIESFROMINVENTORYFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_COSTACCENTRIESFROMINVENTORYComponent, resolver);
    }
} 