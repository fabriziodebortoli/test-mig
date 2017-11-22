import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_FAENTRYService } from './IDD_FAENTRY.service';

@Component({
    selector: 'tb-IDD_FAENTRY',
    templateUrl: './IDD_FAENTRY.component.html',
    providers: [IDD_FAENTRYService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_FAENTRYComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_FAENTRYService,
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
		boService.appendToModelStructure({'Header':['FARsn','PostingDate','RefNo','Currency','DocumentDate','DocNo','LogNo','CustSuppType','CustSupp'],'HKLInvEntr':['Description'],'global':['TotalAmount','Detail','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg','FACode','FADescription','FiscDeprTot','NotFiscDepr','FiscAccumDepr','FiscLostDeprTot','FiscNetBookValue','BalDeprTot','BalAccumDepr','BalNetBookValue','FinDeprTot','FinRenewalReserve','FinAccumDepr','RenewalAccumDepr','FinNetBookValue','AccRenNetBookValue'],'HKLCustSupp':['CompNameComplete'],'Detail':['CodeType','FixedAsset','Qty','Perc','AmountDocCurr','Amount','Notes'],'HKLColFixedAsset':['Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_FAENTRYFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_FAENTRYComponent, resolver);
    }
} 