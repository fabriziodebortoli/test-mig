import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_DEFCLOSINGService } from './IDD_DEFCLOSING.service';

@Component({
    selector: 'tb-IDD_DEFCLOSING',
    templateUrl: './IDD_DEFCLOSING.component.html',
    providers: [IDD_DEFCLOSINGService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_DEFCLOSINGComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_DEFCLOSINGService,
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
        
        		this.bo.appendToModelStructure({'global':['ClosingBalance','ClosingBalDescription','FiscalYearProfit','YearProfitDescri','FiscalYearLoss','YearLossDescri','OpeningBalance','OpenBalDescription','PrevFiscalYearProfit','PrevYearProfitDescri','PrevFiscalYearLoss','PrevYearLossDescri','PostDate','AccrualDate','OpeningPostingDate','OpeningAccrualDate','ResultTransfer','bAccBookAttach','TaxData','TaxDeclData','nCurrentElement','GaugeDescription']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_DEFCLOSINGFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_DEFCLOSINGComponent, resolver);
    }
} 