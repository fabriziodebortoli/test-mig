import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_BALANCEGENERATIONService } from './IDD_BALANCEGENERATION.service';

@Component({
    selector: 'tb-IDD_BALANCEGENERATION',
    templateUrl: './IDD_BALANCEGENERATION.component.html',
    providers: [IDD_BALANCEGENERATIONService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_BALANCEGENERATIONComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_BALANCEGENERATIONService,
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
		boService.appendToModelStructure({'global':['StartingDate','EndingDate','AllKind','Forecast','BalanceNew','BalanceRegenerate','BalanceSchema','Notes','OwnerCompany','Language','Template','Suffix','CompanyId','Currency','FixingDate','Fixing','nCurrentElement','GaugeDescription'],'HKLOwnerCompany':['CompanyName'],'HKLLanguages':['Description'],'HKLConsolidTemplates':['Description'],'HKLCurrenciesCurrObj':['Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_BALANCEGENERATIONFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_BALANCEGENERATIONComponent, resolver);
    }
} 