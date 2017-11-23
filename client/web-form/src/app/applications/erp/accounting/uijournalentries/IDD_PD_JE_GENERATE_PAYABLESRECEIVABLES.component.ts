import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_PD_JE_GENERATE_PAYABLESRECEIVABLESService } from './IDD_PD_JE_GENERATE_PAYABLESRECEIVABLES.service';

@Component({
    selector: 'tb-IDD_PD_JE_GENERATE_PAYABLESRECEIVABLES',
    templateUrl: './IDD_PD_JE_GENERATE_PAYABLESRECEIVABLES.component.html',
    providers: [IDD_PD_JE_GENERATE_PAYABLESRECEIVABLESService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_PD_JE_GENERATE_PAYABLESRECEIVABLESComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_PD_JE_GENERATE_PAYABLESRECEIVABLESService,
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
		boService.appendToModelStructure({'global':['InstallmentCalcFrom','Currency','AmountOpenPayableReceivable','AmountTaxAmountPayableReceivable','ClosingAmountPayableReceivable','CustSuppDescri','PymtTerm','PymtTermDescri','CustSuppBank','CustSuppCA','Job','ContractCode','ProjectCode','ESRReferenceNumber','ESRCheckDigit','ESRNotes']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_PD_JE_GENERATE_PAYABLESRECEIVABLESFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_PD_JE_GENERATE_PAYABLESRECEIVABLESComponent, resolver);
    }
} 