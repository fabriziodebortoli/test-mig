import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_TAXJOURNAL_DEKLARService } from './IDD_TAXJOURNAL_DEKLAR.service';

@Component({
    selector: 'tb-IDD_TAXJOURNAL_DEKLAR',
    templateUrl: './IDD_TAXJOURNAL_DEKLAR.component.html',
    providers: [IDD_TAXJOURNAL_DEKLARService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_TAXJOURNAL_DEKLARComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_TAXJOURNAL_DEKLARService,
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
		boService.appendToModelStructure({'global':['Year','Month','MonthTo','JournalType','OrderBy','ViesType','CreditDebit','PrevPeriodCredit','LastTaxPymt','Amount70','Amount71','Amount80','Amount81','Amount82','FactorRecalculate','Factor_Year','Factor_FromMonth','Factor_ToMonth','FactorSums','FactorSums2','Factor','X1','X2','Position','Folder','Process']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_TAXJOURNAL_DEKLARFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_TAXJOURNAL_DEKLARComponent, resolver);
    }
} 