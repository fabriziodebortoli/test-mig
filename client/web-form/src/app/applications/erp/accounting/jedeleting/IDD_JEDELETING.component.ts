import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_JEDELETINGService } from './IDD_JEDELETING.service';

@Component({
    selector: 'tb-IDD_JEDELETING',
    templateUrl: './IDD_JEDELETING.component.html',
    providers: [IDD_JEDELETINGService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_JEDELETINGComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_JEDELETINGService,
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
		boService.appendToModelStructure({'global':['PureAccounting','IssuedDoc','PurchaseDoc','bFromPostDate','bFromAccrualDate','FromPostDate','ToPostDate','AllTemplate','SelTemplate','FromTemplate','ToTemplate','AllRefNo','SelRefNo','FromRifNo','ToRifNo','AllTaxJournal','SelTaxJournal','FromTaxJournal','ToTaxJournal','AllDocNo','SelDocNo','FromDocNo','ToDocNo','AllLogNo','SelLogNo','FromLogNo','ToLogNo','PymtSchedulesDeleting','FeesDeleting','CostAccDeleting','FADeleting','IntraDeleting','LinkDeleting','Detail'],'Detail':['l_TEnhDeleteComponentJE_P01','TransactionType','l_TEnhDeleteComponentJE_P02','l_TEnhDeleteComponentJE_P03','l_TEnhDeleteComponentJE_P04','l_TEnhDeleteComponentJE_P05','l_TEnhDeleteComponentJE_P06','l_TEnhDeleteComponentJE_P08','Simulation','FinalExpectedDate','l_TEnhDeleteComponentJE_P07','FinalPosting','FinalExpectedDate','FinalPosted','FinalPostingDate','Simulation','SimulationDate','Automatic','Notes']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_JEDELETINGFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_JEDELETINGComponent, resolver);
    }
} 