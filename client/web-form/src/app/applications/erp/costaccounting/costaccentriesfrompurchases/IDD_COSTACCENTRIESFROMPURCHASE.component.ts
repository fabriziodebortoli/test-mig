import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_COSTACCENTRIESFROMPURCHASEService } from './IDD_COSTACCENTRIESFROMPURCHASE.service';

@Component({
    selector: 'tb-IDD_COSTACCENTRIESFROMPURCHASE',
    templateUrl: './IDD_COSTACCENTRIESFROMPURCHASE.component.html',
    providers: [IDD_COSTACCENTRIESFROMPURCHASEService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_COSTACCENTRIESFROMPURCHASEComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_COSTACCENTRIESFROMPURCHASEService,
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
        
        		this.bo.appendToModelStructure({'global':['PurchasesFromPostDate','PurchasesToPostDate','Purchased','AllTaxJournalRadio','TaxJournalRadioSel','TaxJournalFrom','TaxJournalFrom','TaxJournalTo','PurchasesInputDate','PurchasesCostAccPostDate','PurchasesDocPostDate','PurchasesDocDate','PurchasesAccrualEqualToPosting','nCurrentElement','GaugeDescription'],'HKLTaxJournalFrom':['Description','Description'],'HKLTaxJournalTo':['Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_COSTACCENTRIESFROMPURCHASEFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_COSTACCENTRIESFROMPURCHASEComponent, resolver);
    }
} 