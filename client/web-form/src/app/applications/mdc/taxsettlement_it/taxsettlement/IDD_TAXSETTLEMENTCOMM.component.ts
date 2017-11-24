import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_TAXSETTLEMENTCOMMService } from './IDD_TAXSETTLEMENTCOMM.service';

@Component({
    selector: 'tb-IDD_TAXSETTLEMENTCOMM',
    templateUrl: './IDD_TAXSETTLEMENTCOMM.component.html',
    providers: [IDD_TAXSETTLEMENTCOMMService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_TAXSETTLEMENTCOMMComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_TAXSETTLEMENTCOMMService,
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
        
        		this.bo.appendToModelStructure({'global':['Year','Quarter','ExceptionalEvents1','ExceptionalEvents2','ExceptionalEvents3','DifferentDeclarer','PositionCode_XML','DeclarerFiscalCode','CompanyDeclarerFiscalCode','TaxPayerCommitment','IntermediaryCommitment','CommitmentDate','IntermediaryFiscalCode','ConfirmingFlag','ProcessStatus']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_TAXSETTLEMENTCOMMFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_TAXSETTLEMENTCOMMComponent, resolver);
    }
} 