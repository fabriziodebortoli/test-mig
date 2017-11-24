import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_BOLETOSISSUINGPRINTINGService } from './IDD_BOLETOSISSUINGPRINTING.service';

@Component({
    selector: 'tb-IDD_BOLETOSISSUINGPRINTING',
    templateUrl: './IDD_BOLETOSISSUINGPRINTING.component.html',
    providers: [IDD_BOLETOSISSUINGPRINTINGService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_BOLETOSISSUINGPRINTINGComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_BOLETOSISSUINGPRINTINGService,
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
        
        		this.bo.appendToModelStructure({'global':['OperationType','bReprint','AllCustomers','SelCustomer','FromCust','ToCust','bAlsoBlockedCust','FromDueDate','ToDueDate','FromDocNo','ToDocNo','FromIssuingDate','ToIssuingDate','FromNo','ToNo','IssuingDate','BankCode','Instruction','BankCondition','InterestRate','PenalityRate','DiscountRate','ProtestDays','bPrintPreview','bDefPrint','bEMail','bPrintMail'],'HKLBanks':['Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_BOLETOSISSUINGPRINTINGFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_BOLETOSISSUINGPRINTINGComponent, resolver);
    }
} 