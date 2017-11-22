import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_OWNERCOMPANIESService } from './IDD_OWNERCOMPANIES.service';

@Component({
    selector: 'tb-IDD_OWNERCOMPANIES',
    templateUrl: './IDD_OWNERCOMPANIES.component.html',
    providers: [IDD_OWNERCOMPANIESService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_OWNERCOMPANIESComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_OWNERCOMPANIESService,
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
		boService.appendToModelStructure({'OwnerCompanies':['Company','CompanyName','Currency','Language','ExpectedConsolidDayMonth','DefaultTemplate','Notes'],'HKLCurrencies':['Description'],'HKLLanguages':['Description'],'HKLConsolidTemplates':['Description'],'global':['Balances','SendingsMade','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'SendingsMade':['BalanceDate','SendingDate','Template','Notes'],'HKLConsolidTemplatesSendings':['Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_OWNERCOMPANIESFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_OWNERCOMPANIESComponent, resolver);
    }
} 