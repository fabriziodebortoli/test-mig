import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_COSTACCFROMSALESService } from './IDD_COSTACCFROMSALES.service';

@Component({
    selector: 'tb-IDD_COSTACCFROMSALES',
    templateUrl: './IDD_COSTACCFROMSALES.component.html',
    providers: [IDD_COSTACCFROMSALESService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_COSTACCFROMSALESComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_COSTACCFROMSALESService,
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
		boService.appendToModelStructure({'global':['SaleFromPostDate','SaleToPostDate','SalePosted','AllTaxJournalRadio','TaxJournalRadioSel','TaxJournalFrom','TaxJournalFrom','TaxJournalTo','SaleInputPostDate','SalePostDateInCostAcc','SaleDocPostDate','nCurrentElement','GaugeDescription'],'HKLTaxJournalFrom':['Description','Description'],'HKLTaxJournalTo':['Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_COSTACCFROMSALESFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_COSTACCFROMSALESComponent, resolver);
    }
} 