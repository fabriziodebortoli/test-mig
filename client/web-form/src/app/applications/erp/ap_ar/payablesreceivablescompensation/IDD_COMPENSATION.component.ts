import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_COMPENSATIONService } from './IDD_COMPENSATION.service';

@Component({
    selector: 'tb-IDD_COMPENSATION',
    templateUrl: './IDD_COMPENSATION.component.html',
    providers: [IDD_COMPENSATIONService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_COMPENSATIONComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_COMPENSATIONService,
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
        
        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['Customer','strCompensation','CompensationNo','FromDueDate','ToDueDate','Currency','bCompensationPrintPreview','bCompensationPrint','bCompensationSendByMail','bCompensationNothing','bCompensationSendByPostaLite','CompensationPLDeliveryType','CompensationPLPrintType','PostingDate','FixingDate','Fixing','Schedule','DebitTotal','CreditTotal','DifferenceTotal','BlockedImage','LitigationImage'],'Schedule':['l_BlockedBmp','l_LitegationBmp','l_Selected','OpeningDate','l_DebitBalance','l_CreditBalance','l_WithholdingTaxBalance','l_DebitCompensation','l_CreditCompensation','l_DocDate','l_DocNo','l_LogNo','PaymentTerm','InstallmentNo']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_COMPENSATIONFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_COMPENSATIONComponent, resolver);
    }
} 