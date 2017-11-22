import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_MULTICOMPANYBALANCESService } from './IDD_MULTICOMPANYBALANCES.service';

@Component({
    selector: 'tb-IDD_MULTICOMPANYBALANCES',
    templateUrl: './IDD_MULTICOMPANYBALANCES.component.html',
    providers: [IDD_MULTICOMPANYBALANCESService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_MULTICOMPANYBALANCESComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_MULTICOMPANYBALANCESService,
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
		boService.appendToModelStructure({'MultiCompanyBalances':['BalanceSchema','Notes','Template','Company','BalanceDate','Sent','SendingDate','Language','Currency','FixingDate','Fixing','CompanyIdentifier','Suffix'],'HKLConsolidTemplates':['Description'],'HKLCompanyGroups':['CompanyName'],'HKLLanguages':['Description'],'HKLCurrenciesCurrObj':['Description'],'global':['Balances','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'Balances':['ExternalCode','Description','Debit','Credit']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_MULTICOMPANYBALANCESFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_MULTICOMPANYBALANCESComponent, resolver);
    }
} 