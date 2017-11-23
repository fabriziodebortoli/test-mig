import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_ACCINVENTORYBOOKPRINTService } from './IDD_ACCINVENTORYBOOKPRINT.service';

@Component({
    selector: 'tb-IDD_ACCINVENTORYBOOKPRINT',
    templateUrl: './IDD_ACCINVENTORYBOOKPRINT.component.html',
    providers: [IDD_ACCINVENTORYBOOKPRINTService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_ACCINVENTORYBOOKPRINTComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_ACCINVENTORYBOOKPRINTService,
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
        
        		this.bo.appendToModelStructure({'global':['FiscalYear','bFinancialStat','bProfitLoss','bStandardView','bPrevious','bAccInventoryFinStat','bAccInventoryProfitLoss','bCustSuppTrialBalByCode','bCustSuppTrialBalByAccGroup','bDocumentToBeIssRecPrint','bEUBALSheetFinStat','bEUBALSheetFinStatCasc','bEUBALSheetFinStatAbbr','bEUBALSheetProfitLoss','bEUBALSheetMemorandum','bBaselIIFinStat','bBaselIIFinStatCasc','bBaselIIProfitLoss','bFixedAssetsFiscal','bFixedAssetsBalReg','bFixedAssetsNoteFiscal','bFixedAssetsNoteBalReg','bPrintAttachments','bPrintSubscription','bSOSConnector','ContextualHeading','NoPrefix','VideoPage']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_ACCINVENTORYBOOKPRINTFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_ACCINVENTORYBOOKPRINTComponent, resolver);
    }
} 