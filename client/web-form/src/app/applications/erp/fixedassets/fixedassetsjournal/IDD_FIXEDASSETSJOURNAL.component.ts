import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_FIXEDASSETSJOURNALService } from './IDD_FIXEDASSETSJOURNAL.service';

@Component({
    selector: 'tb-IDD_FIXEDASSETSJOURNAL',
    templateUrl: './IDD_FIXEDASSETSJOURNAL.component.html',
    providers: [IDD_FIXEDASSETSJOURNALService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_FIXEDASSETSJOURNALComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_FIXEDASSETSJOURNALService,
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
		boService.appendToModelStructure({'global':['FiscalYear','FiscalRegime','BalanceRegime','AssetsJournal','DetailedHistory','Grouped','HistoryFrom','HistoryYear','AllCategories','SelCategories','FromCategories','ToCategories','AllPurchaseYears','SelPurchaseYears','FromPurchaseYear','ToPurchaseYear','OrderByPurchaseYear','OrderByCategory','Compound','OnePageForCategory','PrintPreviousQuota','Print','ContextualHeading','NoPrefix','StartingPage']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_FIXEDASSETSJOURNALFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_FIXEDASSETSJOURNALComponent, resolver);
    }
} 