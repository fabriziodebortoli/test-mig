import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_OWNEDCOMPANIESService } from './IDD_OWNEDCOMPANIES.service';

@Component({
    selector: 'tb-IDD_OWNEDCOMPANIES',
    templateUrl: './IDD_OWNEDCOMPANIES.component.html',
    providers: [IDD_OWNEDCOMPANIESService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_OWNEDCOMPANIESComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_OWNEDCOMPANIESService,
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
		boService.appendToModelStructure({'OwnedCompanies':['Company','CompanyName','ExpectedConsolidDayMonth','CompanyIdentifier','Notes'],'global':['Balances','SendingsMade','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'SendingsMade':['BalanceDate','SendingDate','Notes']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_OWNEDCOMPANIESFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_OWNEDCOMPANIESComponent, resolver);
    }
} 