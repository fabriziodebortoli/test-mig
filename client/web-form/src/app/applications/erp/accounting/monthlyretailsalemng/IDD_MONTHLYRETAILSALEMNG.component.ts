import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_MONTHLYRETAILSALEMNGService } from './IDD_MONTHLYRETAILSALEMNG.service';

@Component({
    selector: 'tb-IDD_MONTHLYRETAILSALEMNG',
    templateUrl: './IDD_MONTHLYRETAILSALEMNG.component.html',
    providers: [IDD_MONTHLYRETAILSALEMNGService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_MONTHLYRETAILSALEMNGComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_MONTHLYRETAILSALEMNGService,
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
        
        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['Year','Month','Template','TaxJournal','Schema','GeneralTotal','ProgressiveTotal','DailyReg','Result'],'HKLAccTemplates':['Description'],'HKLTaxJournal':['Description'],'Schema':['TaxCode','DebitAccount','CreditAccount'],'HKLTaxCode':['Description'],'HKLDebitAccount':['Description'],'HKLCreditAccount':['Description'],'DailyReg':['VPostDate','VDayName','VTaxAmount01','VTaxAmount02','VTaxAmount03','VTaxAmount04','VTaxAmount05','VTaxAmount06','VTaxAmount07','VTaxAmount08','VTaxAmount09','VTaxAmount10','VTaxAmount11','VTaxAmount12','VTaxAmount13','VTaxAmount14','VTaxAmount15','VTaxAmount16','VTaxAmount17','VTaxAmount18','VTaxAmount19','VTaxAmount20','VTotal','VInvoicesFrom','VInvoicesTo','VCredNotesFrom','VCredNotesTo'],'Result':['EnhSelected','AccTpl','EnhTemplateDescri','PostingDate','TaxJournal','DocNo','EnhTotal','Notes']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_MONTHLYRETAILSALEMNGFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_MONTHLYRETAILSALEMNGComponent, resolver);
    }
} 